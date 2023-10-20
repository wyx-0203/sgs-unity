using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    /// <summary>
    /// 单机回合系统
    /// </summary>
    public class TurnSystem : Singleton<TurnSystem>
    {
        public void Init()
        {
            // 初始化被跳过阶段,设置为否
            foreach (Phase phase in System.Enum.GetValues(typeof(Phase))) SkipPhase.Add(phase, false);

            // CurrentPlayer = SgsMain.Instance.players[0];
            actionQueue = SgsMain.Instance.players.OrderBy(x => x.turnOrder).ToArray();
            CurrentPlayer = actionQueue[0];
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }
        // 当前阶段
        public Phase CurrentPhase { get; private set; }
        private Player[] actionQueue;
        // private int actionQueueIndex;
        // private void GetNextPlayer()
        // {
        //     do CurrentPlayer=CurrentPlayer[(System.Array.IndexOf(actionQueue,CurrentPlayer)+1)%]
        //     return actionQueue[]
        // }

        // 被跳过阶段
        public Dictionary<Phase, bool> SkipPhase { get; set; } = new();

        // public List<Player> finishedList { get; private set; } = new();
        public int Round { get; private set; } = 0;

        public async Task Run()
        {
            Round = 1;
            while (true)
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
                        await ExecutePhase();
                        while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
                    }
                }
                catch (CurrentPlayerDie) { }

                FinishTurnView?.Invoke();
                AfterTurn?.Invoke();

                if (MCTS.Instance.state == MCTS.State.Simulating) throw new FinishSimulation();

                // 额外回合
                if (ExtraTurn != null) await ExecuteExtraTurn();

                int pos = CurrentPlayer.turnOrder;

                do CurrentPlayer = actionQueue[(CurrentPlayer.turnOrder + 1) % actionQueue.Length];
                while (!CurrentPlayer.IsAlive);

                if (CurrentPlayer.turnOrder < pos) Round++;
            }
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
            // 执行阶段开始时view事件
            StartPhaseView?.Invoke();

            await new Delay(0.3f).Run();

            var events = CurrentPlayer.events;

            // 阶段开始时判断是否跳过
            if (SkipPhase[CurrentPhase])
            {
                SkipPhase[CurrentPhase] = false;
                return;
            }
            // 执行阶段开始时事件
            await events.StartPhase[CurrentPhase].Execute();

            // 阶段中判断是否跳过
            if (SkipPhase[CurrentPhase])
            {
                SkipPhase[CurrentPhase] = false;
                return;
            }

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
                    getCardFromPile.InGetCardPhase = true;
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

            // 执行阶段结束时事件
            await events.FinishPhase[CurrentPhase].Execute();
            FinishPhaseView?.Invoke();
        }

        private async Task Play()
        {
            // 重置出杀次数
            CurrentPlayer.杀Count = 0;
            CurrentPlayer.酒Count = 0;
            CurrentPlayer.Use酒 = false;

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

                    var decision1 = PlayDecisions[Random.Range(0, PlayDecisions.Count)];
                    PlayDecisions.Clear();
                    return decision1;
                };


                var decision = await timer.Run(CurrentPlayer);

                if (!decision.action)
                {
                    FinishPerformView?.Invoke();
                    break;
                }

                // 使用技能
                if (decision.skill is Active active)
                {
                    await active.Execute(decision);
                }
                // 使用牌
                else
                {
                    var card = decision.cards[0];
                    if (card is 杀) CurrentPlayer.杀Count++;
                    await card.UseCard(CurrentPlayer, decision.dests);
                }

                FinishPerformView?.Invoke();
            }

            // 重置出杀次数
            CurrentPlayer.杀Count = 0;
            CurrentPlayer.Use酒 = false;

            AfterPlay?.Invoke();
        }

        public List<Decision> PlayDecisions { get; private set; } = new();

        public System.Action AfterTurn { get; set; }
        public System.Action AfterPlay { get; set; }

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

        public Player ExtraTurn { get; set; }
        private async Task ExecuteExtraTurn()
        {
            var t = CurrentPlayer;
            CurrentPlayer = ExtraTurn;
            ExtraTurn = null;

            // 执行回合
            await SgsMain.Instance.MoveSeat(CurrentPlayer);
            StartTurnView?.Invoke();

            for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
            {
                if (!Room.Instance.IsSingle) await Sync();
                await ExecutePhase();
                while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
            }
            FinishTurnView?.Invoke();
            AfterTurn?.Invoke();

            if (ExtraTurn != null) await ExecuteExtraTurn();

            CurrentPlayer = t;
        }

        public List<Phase> ExtraPhase { get; } = new List<Phase>();
        private async Task ExeuteExtraPhase()
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