using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

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
            SkipPhase = new Dictionary<Player, Dictionary<Phase, bool>>();
            foreach (Player player in SgsMain.Instance.players)
            {
                SkipPhase.Add(player, new Dictionary<Phase, bool>());
                foreach (Phase phase in System.Enum.GetValues(typeof(Phase)))
                {
                    SkipPhase[player].Add(phase, false);
                }
            }

            CurrentPlayer = SgsMain.Instance.players[0];
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }
        // 当前阶段
        public Phase CurrentPhase { get; private set; }

        // 被跳过阶段
        public Dictionary<Player, Dictionary<Phase, bool>> SkipPhase { get; set; }

        public int Round { get; private set; } = 0;

        public async Task Run()
        {
            Round = 1;
            while (true)
            {
                // 执行回合
                await SgsMain.Instance.MoveSeat(CurrentPlayer);
                StartTurnView?.Invoke();
                BeforeTurn?.Invoke();

                // 从准备阶段到结束阶段
                for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                {
                    if (!Room.Instance.IsSingle) await Sync();
                    await ExecutePhase();
                    while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
                    if (GameOver.Instance.Check()) return;
                }
                FinishTurnView?.Invoke();
                AfterTurn?.Invoke();

                // 额外回合
                if (ExtraTurn != null) await ExecuteExtraTurn();

                if (CurrentPlayer.position > CurrentPlayer.next.position) Round++;
                CurrentPlayer = CurrentPlayer.next;
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
            if (!CurrentPlayer.IsAlive) return;
            // 执行阶段开始时view事件
            StartPhaseView?.Invoke();

            await new Delay(0.3f).Run();

            var events = CurrentPlayer.events;

            // 阶段开始时判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }
            // 执行阶段开始时事件
            await events.StartPhase[CurrentPhase].Execute();

            // 阶段中判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }

            switch (CurrentPhase)
            {
                // 执行判定阶段
                case Phase.Judge:
                    while (CurrentPlayer.JudgeArea.Count != 0)
                    {
                        await CurrentPlayer.JudgeArea[0].Judge();
                    }
                    break;

                // 执行摸牌阶段
                case Phase.Get:
                    var getCardFromPile = new GetCardFromPile(CurrentPlayer, 2);
                    getCardFromPile.InGetCardPhase = true;
                    await getCardFromPile.Execute();
                    break;

                // 执行出牌阶段
                case Phase.Perform:
                    await Perform();
                    break;

                // 执行弃牌阶段
                case Phase.Discard:
                    var count = CurrentPlayer.HandCardCount - CurrentPlayer.HandCardLimit;
                    if (count > 0) await TimerAction.DiscardFromHand(CurrentPlayer, count);
                    break;
            }

            if (!CurrentPlayer.IsAlive) return;

            // 执行阶段结束时事件
            await events.FinishPhase[CurrentPhase].Execute();
            FinishPhaseView?.Invoke();
        }

        private async Task Perform()
        {
            // 重置出杀次数
            CurrentPlayer.杀Count = 0;
            CurrentPlayer.酒Count = 0;
            CurrentPlayer.Use酒 = false;

            bool action = true;
            while (action && CurrentPlayer.IsAlive && !GameOver.Instance.Check())
            {
                // 暂停线程,显示进度条
                var timer = Timer.Instance;
                timer.Hint = "出牌阶段，请选择一张牌。";
                timer.MaxDest = DestArea.Instance.MaxDest;
                timer.MinDest = DestArea.Instance.MinDest;
                timer.IsValidCard = CardArea.Instance.ValidCard;
                timer.IsValidDest = DestArea.Instance.ValidDest;
                timer.isPerformPhase = true;
                action = await timer.Run(CurrentPlayer, 1, 0);
                timer.isPerformPhase = false;

                if (CurrentPlayer.isAI) action = AI.Instance.Perform();

                if (action)
                {
                    // 使用技能
                    if (timer.skill != null)
                    {
                        await (timer.skill as Active).Execute(timer.dests, timer.cards, "");
                    }
                    // 使用牌
                    else
                    {
                        var card = timer.cards[0];
                        if (card is 杀) CurrentPlayer.杀Count++;
                        await card.UseCard(CurrentPlayer, timer.dests);
                    }
                }

                FinishPerformView?.Invoke();
            }

            // 重置出杀次数
            CurrentPlayer.杀Count = 0;
            CurrentPlayer.Use酒 = false;

            AfterPerform?.Invoke();
        }

        public Action BeforeTurn { get; set; }
        public Action AfterTurn { get; set; }
        public Action AfterPerform { get; set; }

        public void SortDest(List<Player> dests)
        {
            dests.Sort((x, y) =>
            {
                Player i = CurrentPlayer;
                while (true)
                {
                    if (x == i) return -1;
                    else if (y == i) return 1;
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
            BeforeTurn?.Invoke();
            for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
            {
                if (!Room.Instance.IsSingle) await Sync();
                await ExecutePhase();
                while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
                if (GameOver.Instance.Check()) return;
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