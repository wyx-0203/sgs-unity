using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
{
    public enum Phase
    {
        Prepare,    // 准备阶段
        Judge,      // 判定阶段
        Get,        // 摸牌阶段
        Play,       // 出牌阶段
        Discard,    // 弃牌阶段
        End,        // 结束阶段
    }

    /// <summary>
    /// 单机回合系统
    /// </summary>
    public class TurnSystem : Singleton<TurnSystem>
    {
        public void Init()
        {
            // 初始化被跳过阶段,设置为否
            // foreach (Phase phase in System.Enum.GetValues(typeof(Phase))) SkipPhase.Add(phase, false);

            actionQueue = SgsMain.Instance.players.OrderBy(x => x.turnOrder).ToArray();
            CurrentPlayer = actionQueue[0];
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }
        // 当前阶段
        public Phase CurrentPhase { get; private set; }
        private Player[] actionQueue;

        // 被跳过阶段
        public List<Phase> SkipPhase { get; private set; } = new();

        public int Round { get; private set; } = 0;

        public async Task Run()
        {
            Round = 1;

            while (true)
            {
                await ExecuteTurn();
                int pos = CurrentPlayer.turnOrder;
                do CurrentPlayer = actionQueue[(CurrentPlayer.turnOrder + 1) % actionQueue.Length];
                while (!CurrentPlayer.alive || await IsTurnOver(CurrentPlayer));
                if (CurrentPlayer.turnOrder <= pos) Round++;
            }
        }

        private async Task ExecuteTurn()
        {
            // 执行回合
            await SgsMain.Instance.MoveSeat(CurrentPlayer);
            StartTurnView?.Invoke();

            try
            {
                // 从准备阶段到结束阶段
                for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                {
                    if (!Room.Instance.IsSingle) await Sync();
                    try { await ExecutePhase(); }
                    catch (SkipPhaseException) { }
                    while (ExtraPhase.Count > 0) await ExecuteExtraPhase();
                }
            }
            catch (CurrentPlayerDie) { }

            FinishTurnView?.Invoke();
            AfterTurn?.Invoke();
            AfterTurnOnce?.Invoke();
            AfterTurnOnce = null;

            if (MCTS.Instance.state == MCTS.State.Simulating) throw new FinishSimulation();

            // 额外回合
            if (ExtraTurnPlayer != null) await ExecuteExtraTurn();
        }

        private async Task<bool> IsTurnOver(Player player)
        {
            if (player.turnOver)
            {
                await new TurnOver(player).Execute();
                return true;
            }
            return false;
        }

        private async Task Sync()
        {
            if (CurrentPlayer.isSelf)
            {
                var json = new WebsocketMessage { msg_type = "phase" };
                WebSocket.Instance.SendMessage(json);
            }
            await WebSocket.Instance.PopMessage();
        }

        private async Task ExecutePhase()
        {
            // 阶段开始时判断是否跳过
            if (SkipPhase.Contains(CurrentPhase))
            {
                SkipPhase.Remove(CurrentPhase);
                return;
            }

            // 执行阶段开始时view事件
            StartPhaseView?.Invoke();

            await new Delay(0.3f).Run();

            // 执行阶段开始时事件
            await EventSystem.Instance.Invoke(x => x.OnEveryPhaseStart, new Tuple<Player, Phase>(CurrentPlayer, CurrentPhase), () =>
            {
                if (!CurrentPlayer.alive) throw new CurrentPlayerDie();
            });

            switch (CurrentPhase)
            {
                // 执行判定阶段
                case Phase.Judge:
                    while (CurrentPlayer.JudgeCards.Count != 0)
                    {
                        await CurrentPlayer.JudgeCards[0].Judge();
                    }
                    break;

                // 执行摸牌阶段
                case Phase.Get:
                    var getCardFromPile = new GetCardFromPile(CurrentPlayer, 2);
                    getCardFromPile.inGetPhase = true;
                    await getCardFromPile.Execute();
                    break;

                // 执行出牌阶段
                case Phase.Play:
                    await Play();
                    break;

                // 执行弃牌阶段
                case Phase.Discard:
                    var count = CurrentPlayer.HandCardCount - CurrentPlayer.HandCardLimit;
                    if (count > 0) await TimerAction.DiscardFromHand(CurrentPlayer, count);
                    break;
            }
            if (!CurrentPlayer.alive) throw new CurrentPlayerDie();

            // 执行阶段结束时事件
            await EventSystem.Instance.Invoke(x => x.OnEveryPhaseOver, new Tuple<Player, Phase>(CurrentPlayer, CurrentPhase), () =>
            {
                if (!CurrentPlayer.alive) throw new CurrentPlayerDie();
            });
            FinishPhaseView?.Invoke();
        }

        private async Task Play()
        {
            // 重置出杀次数
            CurrentPlayer.shaCount = 0;
            CurrentPlayer.jiuCount = 0;
            CurrentPlayer.useJiu = false;

            while (true)
            {
                // 暂停线程,显示进度条
                var timer = Timer.Instance;
                timer.hint = "出牌阶段，请选择一张牌。";
                timer.maxCard = 1;
                timer.minCard = 1;
                timer.maxDest = DestArea.Instance.MaxDest;
                timer.minDest = DestArea.Instance.MinDest;
                timer.isValidCard = CardArea.Instance.ValidCard;
                timer.isValidDest = DestArea.Instance.ValidDest;
                timer.type = Timer.Type.PlayPhase;

                timer.DefaultAI = () =>
                {
                    var validCards = CurrentPlayer.HandCards.Where(x => CardArea.Instance.ValidCard(x));
                    foreach (var i in validCards)
                    {
                        timer.temp.cards.Add(i);

                        if (timer.maxDest() > 0)
                        {
                            var dests = AI.GetValidDest();
                            if (dests.Count == 0 || dests[0].team == CurrentPlayer.team)
                            {
                                timer.temp.cards.Clear();
                                continue;
                            }

                            timer.temp.dests.AddRange(dests);
                        }
                        timer.temp.action = true;
                        PlayDecisions.Add(timer.SaveTemp());
                    }

                    var skills = CurrentPlayer.skills.Where(x => x is Active && x.IsValid);
                    foreach (var i in skills)
                    {
                        timer.temp.skill = i;
                        var decision = i.AIDecision();
                        if (decision.action) PlayDecisions.Add(decision);
                    }

                    if (PlayDecisions.Count == 0 || !AI.CertainValue) PlayDecisions.Add(new Decision());

                    var decision1 = PlayDecisions[UnityEngine.Random.Range(0, PlayDecisions.Count)];
                    PlayDecisions.Clear();
                    return decision1;
                };


                var decision = await timer.Run(CurrentPlayer);

                if (!decision.action) break;

                // 使用技能
                if (decision.skill is Active active)
                {
                    await active.Use(decision);
                }
                // 使用牌
                else
                {
                    var card = decision.cards[0];
                    if (card is 杀) CurrentPlayer.shaCount++;
                    await card.UseCard(CurrentPlayer, decision.dests);
                }

                FinishPerformView?.Invoke();
            }

            // 重置出杀次数
            CurrentPlayer.shaCount = 0;
            CurrentPlayer.useJiu = false;

            FinishPerformView?.Invoke();
            AfterPlay?.Invoke();
            AfterPlayOnce?.Invoke();
            AfterPlayOnce = null;
        }

        public List<Decision> PlayDecisions { get; private set; } = new();

        public System.Action AfterTurn { get; set; }
        public System.Action AfterPlay { get; set; }
        public System.Action AfterTurnOnce { get; set; }
        public System.Action AfterPlayOnce { get; set; }

        public void SortDest(List<Player> dests)
        {
            dests.Sort((x, y) =>
            {
                Player i = CurrentPlayer;
                while (true)
                {
                    if (x == i) return -1;
                    if (y == i) return 1;
                    i = i.next;
                }
            });
        }

        public Player ExtraTurnPlayer { get; set; }
        private async Task ExecuteExtraTurn()
        {
            var t = CurrentPlayer;
            CurrentPlayer = ExtraTurnPlayer;
            ExtraTurnPlayer = null;

            await ExecuteTurn();

            CurrentPlayer = t;
        }

        public List<Phase> ExtraPhase { get; } = new();
        private async Task ExecuteExtraPhase()
        {
            var t = CurrentPhase;
            CurrentPhase = ExtraPhase[0];
            ExtraPhase.RemoveAt(0);

            // 执行阶段
            await ExecutePhase();

            CurrentPhase = t;
        }

        public UnityAction StartTurnView { get; set; }
        public UnityAction FinishTurnView { get; set; }
        public UnityAction StartPhaseView { get; set; }
        public UnityAction FinishPhaseView { get; set; }
        public UnityAction FinishPerformView { get; set; }
    }
}