using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GameCore
{
    public abstract class DelayScheme : Card
    {
        protected override async Task UseForeachDest()
        {
            CardPile.Instance.RemoveToDiscard(this);
            AddToJudgeArea(dest);
            await Task.Yield();
        }

        public Player Owner { get; private set; }

        public void AddToJudgeArea(Player owner)
        {
            Owner = owner;
            src = owner;
            Owner.JudgeCards.Insert(0, this);
            if (!MCTS.Instance.isRunning) AddJudgeView?.Invoke(this);
        }

        public void RemoveToJudgeArea()
        {
            Owner.JudgeCards.Remove(this);
            if (!MCTS.Instance.isRunning) RemoveJudgeView?.Invoke(this);
        }

        public abstract Task OnJudgePhase();

        protected Card judgeCard;

        public static Action<DelayScheme> AddJudgeView { get; set; }
        public static Action<DelayScheme> RemoveJudgeView { get; set; }
    }

    public class 乐不思蜀 : DelayScheme
    {
        public 乐不思蜀()
        {
            type = "延时锦囊";
            name = "乐不思蜀";
        }

        public override async Task OnJudgePhase()
        {
            CardPile.Instance.AddToDiscard(this);
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this)) return;
            judgeCard = await Judge.Execute();

            if (judgeCard.suit != "红桃") TurnSystem.Instance.SkipPhase.Add(Phase.Play);
        }
    }

    public class 兵粮寸断 : DelayScheme
    {
        public 兵粮寸断()
        {
            type = "延时锦囊";
            name = "兵粮寸断";
        }

        public override async Task OnJudgePhase()
        {
            CardPile.Instance.AddToDiscard(this);
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this)) return;
            judgeCard = await Judge.Execute();

            if (judgeCard.suit != "草花") TurnSystem.Instance.SkipPhase.Add(Phase.Get);
        }
    }

    public class 闪电 : DelayScheme
    {
        public 闪电()
        {
            type = "延时锦囊";
            name = "闪电";
        }

        protected override async Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            await Task.Yield();
        }

        public override async Task OnJudgePhase()
        {
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this))
            {
                AddToJudgeArea(Owner.next);
                return;
            }
            judgeCard = await Judge.Execute();

            if (judgeCard.suit == "黑桃" && judgeCard.weight >= 2 && judgeCard.weight <= 9)
            {
                CardPile.Instance.AddToDiscard(this);
                await new Damaged(Owner, null, this, 3, Damaged.Type.Thunder).Execute();
            }
            else AddToJudgeArea(Owner.next);
        }
    }
}