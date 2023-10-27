using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
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
            Src = owner;
            Owner.JudgeCards.Insert(0, this);
            if (!MCTS.Instance.isRunning) AddJudgeView?.Invoke(this);
        }

        public void RemoveToJudgeArea()
        {
            Owner.JudgeCards.Remove(this);
            if (!MCTS.Instance.isRunning) RemoveJudgeView?.Invoke(this);
        }

        public abstract Task Judge();

        protected Card judgeCard;

        public static UnityAction<DelayScheme> AddJudgeView { get; set; }
        public static UnityAction<DelayScheme> RemoveJudgeView { get; set; }
    }

    public class 乐不思蜀 : DelayScheme
    {
        public 乐不思蜀()
        {
            type = "延时锦囊";
            name = "乐不思蜀";
        }

        public override async Task Judge()
        {
            CardPile.Instance.AddToDiscard(this);
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this)) return;
            judgeCard = await new Judge().Execute();

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

        public override async Task Judge()
        {
            CardPile.Instance.AddToDiscard(this);
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this)) return;
            judgeCard = await new Judge().Execute();

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
            if (Dests.Count == 0) Dests.Add(Src);
            await Task.Yield();
        }

        public override async Task Judge()
        {
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this))
            {
                AddToJudgeArea(Owner.next);
                return;
            }
            judgeCard = await new Judge().Execute();

            if (judgeCard.suit == "黑桃" && judgeCard.weight >= 2 && judgeCard.weight <= 9)
            {
                CardPile.Instance.AddToDiscard(this);
                await new Damaged(Owner, null, this, 3, Damaged.Type.Thunder).Execute();
            }
            else AddToJudgeArea(Owner.next);
        }
    }
}