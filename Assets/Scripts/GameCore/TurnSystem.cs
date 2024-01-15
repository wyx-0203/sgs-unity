using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore
{

    /// <summary>
    /// 单机回合系统
    /// </summary>
    public class TurnSystem : Singleton<TurnSystem>
    {
        public void Init()
        {
            // 初始化被跳过阶段,设置为否
            foreach (Phase phase in System.Enum.GetValues(typeof(Phase))) BeforePhaseExecute.Add(phase, null);

            actionQueue = Game.Instance.players.OrderBy(x => x.turnOrder).ToArray();
            CurrentPlayer = actionQueue[0];
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }
        // 当前阶段
        public Phase CurrentPhase { get; private set; }
        private Player[] actionQueue;

        // 被跳过阶段
        public List<Phase> SkipPhase { get; } = new();

        /// <summary>
        /// 游戏轮数
        /// </summary>
        public int round { get; private set; } = 0;

        public async Task Run()
        {
            round = 1;

            while (true)
            {
                await ExecuteTurn();
                int pos = CurrentPlayer.turnOrder;
                do CurrentPlayer = actionQueue[(CurrentPlayer.turnOrder + 1) % actionQueue.Length];
                while (!CurrentPlayer.alive || await IsTurnOver(CurrentPlayer));
                if (CurrentPlayer.turnOrder <= pos) round++;
            }
        }

        private async Task ExecuteTurn()
        {
            // 执行回合
            // await Main.Instance.MoveSeat(CurrentPlayer);
            EventSystem.Instance.Send(new StartTurn { player = CurrentPlayer.position });

            try
            {
                // 从准备阶段到结束阶段
                for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                {
                    // if (!Room.Instance.IsSingle) await Sync();
                    try { await ExecutePhase(); }
                    catch (SkipPhaseException) { }
                    while (ExtraPhase.Count > 0) await ExecuteExtraPhase();
                }
            }
            catch (CurrentPlayerDie) { }

            EventSystem.Instance.Send(new FinishTurn { player = CurrentPlayer.position });
            AfterTurn?.Invoke();
            AfterTurn = null;

            // if (MCTS.Instance.state == MCTS.State.Simulating) throw new FinishSimulation();

            // 额外回合
            if (ExtraTurnPlayer != null) await ExecuteExtraTurn();
        }

        private async Task<bool> IsTurnOver(Player player)
        {
            if (player.isTurnOver)
            {
                await new TurnOver(player).Execute();
                return true;
            }
            return false;
        }

        // private async Task Sync()
        // {
        //     if (CurrentPlayer.isSelf)
        //     {
        //         var json = new Model.Message { type = "phase" };
        //         WebSocket.Instance.SendMessage(json);
        //     }
        //     await WebSocket.Instance.PopMessage();
        // }

        private async Task ExecutePhase()
        {
            // 阶段开始时判断是否跳过
            if (SkipPhase.Contains(CurrentPhase))
            {
                SkipPhase.Remove(CurrentPhase);
                return;
            }

            // 执行阶段开始时view事件
            EventSystem.Instance.Send(new StartPhase { player = CurrentPlayer.position, phase = CurrentPhase });

            await new Delay(0.3f).Run();

            // 执行阶段开始时事件
            await Triggered.Invoke(x => x.OnEveryPhaseStart, new Tuple<Player, Phase>(CurrentPlayer, CurrentPhase), () =>
            {
                if (!CurrentPlayer.alive) throw new CurrentPlayerDie();
            });

            if (BeforePhaseExecute[CurrentPhase] != null)
            {
                await BeforePhaseExecute[CurrentPhase]();
                BeforePhaseExecute[CurrentPhase] = null;
            }

            switch (CurrentPhase)
            {
                // 执行判定阶段
                case Phase.Judge:
                    while (CurrentPlayer.JudgeCards.Count != 0)
                    {
                        await CurrentPlayer.JudgeCards[0].OnJudgePhase();
                    }
                    break;

                // 执行摸牌阶段
                case Phase.Get:
                    var getCardFromPile = new DrawCard(CurrentPlayer, 2);
                    getCardFromPile.inGetPhase = true;
                    await getCardFromPile.Execute();
                    break;

                // 执行出牌阶段
                case Phase.Play:
                    await Play();
                    break;

                // 执行弃牌阶段
                case Phase.Discard:
                    var count = CurrentPlayer.handCardsCount - CurrentPlayer.handCardsLimit;
                    if (count > 0) await TimerAction.DiscardFromHand(CurrentPlayer, count);
                    break;
            }
            if (!CurrentPlayer.alive) throw new CurrentPlayerDie();

            // 执行阶段结束时事件
            await Triggered.Invoke(x => x.OnEveryPhaseOver, new Tuple<Player, Phase>(CurrentPlayer, CurrentPhase), () =>
            {
                if (!CurrentPlayer.alive) throw new CurrentPlayerDie();
            });
            EventSystem.Instance.Send(new FinishPhase { player = CurrentPlayer.position });
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
                // var timer = Timer.Instance;
                // timer.hint = "出牌阶段，请选择一张牌。";
                // timer.maxCard = 1;
                // timer.minCard = 1;
                // timer.maxDest = DestArea.Instance.MaxDest;
                // timer.minDest = DestArea.Instance.MinDest;
                // timer.isValidCard = CardArea.Instance.ValidCard;
                // timer.isValidDest = DestArea.Instance.ValidDest;
                // timer.type = Timer.Type.InPlayPhase;

                // timer.defaultAI = () =>
                // {
                //     var validCards = CurrentPlayer.handCards.Where(x => CardArea.Instance.ValidCard(x));
                //     foreach (var i in validCards)
                //     {
                //         timer.temp.cards.Add(i);

                //         if (timer.maxDest() > 0)
                //         {
                //             var dests = AI.GetValidDest();
                //             if (dests.Count == 0 || dests[0].team == CurrentPlayer.team)
                //             {
                //                 timer.temp.cards.Clear();
                //                 continue;
                //             }

                //             timer.temp.dests.AddRange(dests);
                //         }
                //         timer.temp.action = true;
                //         PlayDecisions.Add(timer.SaveTemp());
                //     }

                //     var skills = CurrentPlayer.skills.Where(x => x is Active && x.IsValid);
                //     foreach (var i in skills)
                //     {
                //         timer.temp.skill = i;
                //         var decision = i.AIDecision();
                //         if (decision.action) PlayDecisions.Add(decision);
                //     }

                //     if (PlayDecisions.Count == 0 || !AI.CertainValue) PlayDecisions.Add(new Decision());

                //     var decision1 = PlayDecisions[UnityEngine.Random.Range(0, PlayDecisions.Count)];
                //     PlayDecisions.Clear();
                //     return decision1;
                // };


                var decision = await new PlayQuery
                {
                    player = CurrentPlayer,
                    hint = "出牌阶段，请选择一张牌。",
                    maxCard = 1,
                    minCard = 1,
                    isValidCard = card => card.IsValid(),
                    maxDestForCard = card => card.MaxDest(),
                    minDestForCard = card => card.MinDest(),
                    isValidDestForCard = (player, card) => card.IsValidDest(player),
                    type = Model.SinglePlayQuery.Type.PlayPhase,
                    defaultAI = () => new()
                    // {
                    //     // var validCards = CurrentPlayer.handCards.Where(x => CardArea.Instance.ValidCard(x));
                    //     var startPlay=timer.startPlay;
                    //     var validCards = CurrentPlayer.cards.Where(x => startPlay.isValidCard(x));
                    //     foreach (var i in validCards)
                    //     {
                    //         timer.temp.cards.Add(i);
                    //         var temp = new Decision { cards = new List<Card> { i } };

                    //         if (startPlay.maxDestForCard(i) > 0)
                    //         {
                    //             var dests = AI.Instance.GetValidDest(i);
                    //             if (dests.Count == 0 || dests[0].team == CurrentPlayer.team)
                    //             {
                    //                 // timer.temp.cards.Clear();
                    //                 continue;
                    //             }

                    //             timer.temp.dests.AddRange(dests);
                    //         }
                    //         timer.temp.action = true;
                    //         PlayDecisions.Add(timer.SaveTemp());
                    //     }

                    //     var skills = CurrentPlayer.skills.Where(x => x is Active && x.IsValid);
                    //     foreach (var i in skills)
                    //     {
                    //         timer.temp.skill = i;
                    //         var decision = i.AIDecision();
                    //         if (decision.action) PlayDecisions.Add(decision);
                    //     }

                    //     if (PlayDecisions.Count == 0 || !AI.CertainValue) PlayDecisions.Add(new Decision());

                    //     var decision1 = PlayDecisions[UnityEngine.Random.Range(0, PlayDecisions.Count)];
                    //     PlayDecisions.Clear();
                    //     return decision1;
                    // }
                }.Run();

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
                    await card.UseCard(CurrentPlayer, decision.dests);
                }

                EventSystem.Instance.Send(new FinishOncePlay { player = CurrentPlayer.position });
            }

            // 重置出杀次数
            // CurrentPlayer.shaCount = 0;
            // CurrentPlayer.jiuCount = 0;
            // CurrentPlayer.useJiu = false;

            AfterPlay?.Invoke();
            AfterPlay = null;
        }

        public List<PlayDecision> PlayDecisions { get; } = new();

        /// <summary>
        /// 回合结束后事件 (用于重置数据)
        /// </summary>
        public Action AfterTurn;

        /// <summary>
        /// 出牌结束后事件 (用于重置数据)
        /// </summary>
        public Action AfterPlay;

        /// <summary>
        /// 阶段开始时事件 (刘禅、廖化)
        /// </summary>
        public Dictionary<Phase, Func<Task>> BeforePhaseExecute { get; } = new();

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

        // public Action StartTurnView;
        // public Action FinishTurnView;
        // public Action StartPhaseView;
        // public Action FinishPhaseView;
        // public Action FinishPlayView;
    }
}