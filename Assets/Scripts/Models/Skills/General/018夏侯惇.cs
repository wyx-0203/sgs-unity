using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 刚烈 : Triggered
    {
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
                var decision = await WaitDecision();
                if (!decision.action) return;
                await Execute(decision);

                var card = await new Judge().Execute();

                // 红色
                if (card.isRed) await new Damaged(damaged.Src, Src).Execute();

                // 黑色
                else
                {
                    if (damaged.Src.CardCount == 0) continue;
                    CardPanel.Instance.Title = "刚烈";
                    CardPanel.Instance.Hint = "对" + damaged.Src + "发动刚烈，弃置其一张牌";
                    var c = await TimerAction.SelectOneCard(Src, damaged.Src);
                    await new Discard(damaged.Src, c).Execute();
                }
            }
        }

        private Player dest;

        public override Decision AIDecision() => dest.team != Src.team || !AI.CertainValue ? AI.TryAction() : new();
    }
    public class 清俭 : Triggered
    {
        public override int TimeLimit => 1;

        public override void OnEnable()
        {
            Src.events.AfterGetCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterGetCard.RemoveEvent(Src);
        }

        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override bool IsValidDest(Player dest) => dest != Src;

        public async Task Execute(GetCard getCard)
        {
            if (TurnSystem.Instance.Round == 0) return;
            if (!IsValid || getCard is GetCardFromPile getCardFromPile && getCardFromPile.InGetCardPhase) return;

            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            dest = decision.dests[0];
            offset = 0;

            if (decision.cards.Find(x => x.type == "基本牌") != null) offset++;
            if (decision.cards.Find(x => x.type == "锦囊牌" || x.type == "延时锦囊") != null) offset++;
            if (decision.cards.Find(x => x is Equipment) != null) offset++;
            dest.HandCardLimitOffset += offset;

            await new GetCardFromElse(dest, Src, decision.cards).Execute();
        }

        private Player dest;
        private int offset;

        protected override void ResetAfterTurn()
        {
            base.ResetAfterTurn();
            if (dest is null) return;

            dest.HandCardLimitOffset -= offset;
            dest = null;
        }

        public override Decision AIDecision()
        {
            var cards = AI.GetRandomCard();
            var dests = AI.GetDestByTeam(Src.team).ToList();
            if (cards.Count == 0 || dests.Count == 0 || !AI.CertainValue) return new();

            return new Decision { action = true, cards = cards, dests = AI.Shuffle(dests) };
        }
    }
}
