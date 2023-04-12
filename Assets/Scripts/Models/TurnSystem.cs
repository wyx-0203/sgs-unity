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

        public int Round { get; private set; } = 1;

        public async Task Run()
        {
            while (true)
            {
                // 执行回合
                await SgsMain.Instance.MoveSeat(CurrentPlayer);
                StartTurnView?.Invoke(this);
                BeforeTurn?.Invoke();
                for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                {
                    if (!Room.Instance.IsSingle) await Sync();
                    await ExecutePhase();
                    while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
                    if (GameOver.Instance.Check()) return;
                }
                FinishTurnView?.Invoke(this);
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
                WS.Instance.SendJson(json);
            }
            await WS.Instance.PopMsg();
        }

        private async Task ExecutePhase()
        {
            if (!CurrentPlayer.IsAlive) return;
            // 执行阶段开始时view事件
            StartPhaseView?.Invoke(this);

            // #if UNITY_EDITOR
            //             await Task.Delay(300);
            // #endif
            await new Delay(0.3f).Run();

            var playerEvents = CurrentPlayer.events;

            // 阶段开始时判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }
            // 执行阶段开始时事件
            await playerEvents.StartPhase[CurrentPhase].Execute();

            // 阶段中判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }
            // await playerEvents.phaseEvents[CurrentPhase].Execute();

            switch (CurrentPhase)
            {
                case Phase.Judge:
                    while (CurrentPlayer.JudgeArea.Count != 0)
                    {
                        await CurrentPlayer.JudgeArea[0].Judge();
                    }
                    break;
                // 执行摸牌阶段
                case Phase.Get:

                    var act = new GetCardFromPile(CurrentPlayer, 2);
                    act.InGetCardPhase = true;
                    await act.Execute();
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
            await playerEvents.FinishPhase[CurrentPhase].Execute();
            FinishPhaseView?.Invoke(this);
        }

        // public bool IsDone { get; set; }

        private async Task Perform()
        {
            // 重置出杀次数
            CurrentPlayer.ShaCount = 0;
            CurrentPlayer.酒Count = 0;
            CurrentPlayer.Use酒 = false;
            // 重置使用技能次数
            // foreach (var i in CurrentPlayer.skills.Values) if (i is Active) (i as Active).Time = 0;

            bool action = true;
            while (action && CurrentPlayer.IsAlive)
            {
                if (GameOver.Instance.Check()) return;
                // 暂停线程,显示进度条
                var timerTask = Timer.Instance;
                timerTask.Hint = "出牌阶段，请选择一张牌。";
                timerTask.MaxDest = DestArea.Instance.MaxDest;
                timerTask.MinDest = DestArea.Instance.MinDest;
                timerTask.IsValidCard = CardArea.Instance.ValidCard;
                timerTask.IsValidDest = DestArea.Instance.ValidDest;
                timerTask.isPerformPhase = true;
                action = await timerTask.Run(CurrentPlayer, 1, 0);
                timerTask.isPerformPhase = false;

                if (CurrentPlayer.isAI) action = AI.Instance.Perform();

                if (action)
                {
                    // 使用技能
                    if (timerTask.Skill != "")
                    {
                        var skill = CurrentPlayer.FindSkill(timerTask.Skill) as Active;
                        await skill.Execute(timerTask.Dests, timerTask.Cards, "");
                    }
                    // 使用牌
                    else
                    {
                        var card = timerTask.Cards[0];
                        if (card is 杀) CurrentPlayer.ShaCount++;
                        await card.UseCard(CurrentPlayer, timerTask.Dests);
                    }
                }

                FinishPerformView?.Invoke(this);
            }

            // 重置出杀次数
            CurrentPlayer.ShaCount = 0;
            CurrentPlayer.Use酒 = false;

            AfterPerform?.Invoke();
        }

        public Action BeforeTurn;
        public Action AfterTurn;
        public Action AfterPerform;

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
            StartTurnView?.Invoke(this);
            BeforeTurn?.Invoke();
            for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
            {
                if (!Room.Instance.IsSingle) await Sync();
                await ExecutePhase();
                while (ExtraPhase.Count > 0) await ExeuteExtraPhase();
                if (GameOver.Instance.Check()) return;
            }
            FinishTurnView?.Invoke(this);
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

        public UnityAction<TurnSystem> StartTurnView { get; set; }
        public UnityAction<TurnSystem> FinishTurnView { get; set; }
        public UnityAction<TurnSystem> StartPhaseView { get; set; }
        public UnityAction<TurnSystem> FinishPhaseView { get; set; }
        public UnityAction<TurnSystem> FinishPerformView { get; set; }
    }
}