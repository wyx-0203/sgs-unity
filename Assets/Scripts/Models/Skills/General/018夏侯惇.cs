using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 刚烈 : Triggered
    {
        public 刚烈(Player src) : base(src) { }

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null || damaged.Src == Src) return;
            dest = damaged.Src;

            for (int i = 0; i < -damaged.Value; i++)
            {
                if (!await base.ShowTimer()) return;
                Execute();

                var card = await new Judge().Execute();
                if (card.Suit == "红桃" || card.Suit == "方片") await new Damaged(damaged.Src, Src).Execute();
                else if (damaged.Src.CardCount > 0)
                {
                    CardPanel.Instance.Title = "刚烈";
                    CardPanel.Instance.Hint = "对" + damaged.Src.posStr + "号位发动刚烈，弃置其一张牌";
                    var c = await CardPanel.Instance.SelectCard(Src, damaged.Src);
                    await new Discard(damaged.Src, new List<Card> { c }).Execute();
                }
            }
        }

        private Player dest;

        protected override bool AIResult()
        {
            bool result = dest.team != Src.team;
            if (result) AI.Instance.SelectDest();
            return result;
        }
    }
    public class 清俭 : Triggered
    {
        public 清俭(Player src) : base(src) { }
        public override int TimeLimit => 1;

        public override void OnEnable()
        {
            Src.events.AfterGetCard.AddEvent(Src, Execute);
            TurnSystem.Instance.AfterTurn += Reset;
        }

        public override void OnDisable()
        {
            Src.events.AfterGetCard.RemoveEvent(Src);
            TurnSystem.Instance.AfterTurn -= Reset;
        }

        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override bool IsValidDest(Player dest) => dest != Src;

        public async Task Execute(GetCard getCard)
        {
            if (!IsValid || getCard is GetCardFromPile && (getCard as GetCardFromPile).InGetCardPhase) return;
            if (TurnSystem.Instance.Round == 0) return;

            if (!await base.ShowTimer()) return;
            Execute();

            dest = Timer.Instance.dests[0];
            offset = 0;

            if (Timer.Instance.cards.Find(x => x.Type == "基本牌") != null) offset++;
            if (Timer.Instance.cards.Find(x => x.Type == "锦囊牌" || x.Type == "延时锦囊") != null) offset++;
            if (Timer.Instance.cards.Find(x => x is Equipage) != null) offset++;
            dest.HandCardLimitOffset += offset;

            await new GetCardFromElse(dest, Src, Timer.Instance.cards).Execute();
        }

        private Player dest;
        private int offset;

        protected override void Reset()
        {
            base.Reset();
            if (dest != null)
            {
                dest.HandCardLimitOffset -= offset;
                dest = null;
            }
        }

        protected override bool AIResult() => false;
    }
}
