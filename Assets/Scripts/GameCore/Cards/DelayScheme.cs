using System;
using System.Threading.Tasks;

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

        public Player owner { get; private set; }

        public void AddToJudgeArea(Player _owner)
        {
            owner = _owner;
            src = owner;
            owner.JudgeCards.Insert(0, this);
            EventSystem.Instance.Send(new Model.AddJudgeCard
            {
                player = owner.position,
                card = id
            });
        }

        public void RemoveToJudgeArea()
        {
            owner.JudgeCards.Remove(this);
            EventSystem.Instance.Send(new Model.RemoveJudgeCard
            {
                player = owner.position,
                card = id
            });
        }

        public virtual async Task OnJudgePhase()
        {
            CardPile.Instance.AddToDiscard(this, owner);
            RemoveToJudgeArea();
            if (await 无懈可击.Call(this)) return;
            judgeCard = await Judge.Execute();
        }

        protected Card judgeCard;
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
            await base.OnJudgePhase();
            if (judgeCard.suit != "红桃") TurnSystem.Instance.SkipPhase.Add(Model.Phase.Play);
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => src != player && player.JudgeCards.Find(x => x is 乐不思蜀) is null;
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
            await base.OnJudgePhase();
            if (judgeCard.suit != "草花") TurnSystem.Instance.SkipPhase.Add(Model.Phase.Get);
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => src.GetDistance(player) == 1 && player.JudgeCards.Find(x => x is 兵粮寸断) is null;
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
                AddToJudgeArea(owner.next);
                return;
            }
            judgeCard = await Judge.Execute();

            if (judgeCard.suit == "黑桃" && judgeCard.weight >= 2 && judgeCard.weight <= 9)
            {
                CardPile.Instance.AddToDiscard(this, owner);
                await new Damage(owner, null, this, 3, Model.Damage.Type.Thunder).Execute();
            }
            else AddToJudgeArea(owner.next);
        }

        public override bool IsValid() => src.JudgeCards.Find(x => x is 闪电) is null;
    }
}