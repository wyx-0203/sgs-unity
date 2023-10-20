using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 恩怨 : Triggered
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            Src.events.AfterGetCard.AddEvent(Src, Execute);
            Src.events.AfterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterGetCard.RemoveEvent(Src);
            Src.events.AfterDamaged.RemoveEvent(Src);
        }

        public async Task Execute(GetCard getCard)
        {
            if (getCard is not GetCardFromElse getCardFromElse) return;
            if (getCardFromElse.Cards.Count < 2) return;
            dest = getCardFromElse.Dest;
            triggerByDamage = false;

            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            await new GetCardFromPile(getCardFromElse.Dest, 1).Execute();
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null) return;
            dest = damaged.Src;
            for (int i = 0; i < -damaged.Value; i++)
            {
                triggerByDamage = true;
                var decision = await WaitDecision();
                if (!decision.action) return;
                await Execute(decision);

                Timer.Instance.hint = "交给法正一张手牌，或失去一点体力";
                Timer.Instance.isValidCard = card => card.IsHandCard;
                Timer.Instance.DefaultAI = () =>
                {
                    var cards = AI.GetRandomCard();
                    if (cards.Count == 0 || !AI.CertainValue) return new();
                    else return new Decision { action = true, cards = cards };
                };
                decision = await Timer.Instance.Run(damaged.Src, 1, 0);

                if (decision.action)
                {
                    await new GetCardFromElse(Src, damaged.Src, decision.cards).Execute();
                    if (decision.cards[0].suit != "红桃") await new GetCardFromPile(Src, 1).Execute();
                }
                else await new UpdateHp(damaged.Src, -1).Execute();
            }
        }

        private Player dest;
        private bool triggerByDamage;

        public override Decision AIDecision()
        {
            if (!triggerByDamage && dest.team != Src.team && AI.CertainValue) return new();
            return AI.TryAction();
        }
    }

    public class 眩惑 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.FinishPhase[Phase.Get].AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.FinishPhase[Phase.Get].RemoveEvent(Src);
        }

        public override int MaxCard => 2;
        public override int MinCard => 2;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidCard(Card card) => card.IsHandCard;

        public override bool IsValidDest(Player dest) => Timer.Instance.temp.dests.Count > 0 || dest != Src;

        public async Task Execute()
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            var dest0 = decision.dests[0];
            var dest1 = decision.dests[1];
            await new GetCardFromElse(dest0, Src, decision.cards).Execute();
            var list = new List<Card>
            {
                Card.Convert<杀>(),
                Card.Convert<火杀>(),
                Card.Convert<雷杀>(),
                Card.Convert<决斗>(),
            };
            Timer.Instance.multiConvert.AddRange(list);
            Timer.Instance.isValidCard = card => true;
            Timer.Instance.isValidDest = player => player == dest1;
            Timer.Instance.DefaultAI = () =>
            {
                if (!AI.CertainValue) return new();
                var card = AI.Shuffle(list)[0];
                return new Decision { action = true, converted = card, dests = new List<Player> { dest1 } };
            };
            decision = await Timer.Instance.Run(dest0, 0, 1);
            if (decision.action) await decision.converted.UseCard(dest0, decision.dests);
            else await new GetCardFromElse(Src, dest0, new List<Card>(dest0.HandCards)).Execute();
        }

        public override Decision AIDecision()
        {
            var cards = AI.GetRandomCard();
            var dests = AI.GetDestByTeam(Src.team);
            if (cards.Count < 2 || dests.Count() == 0) return new();

            var dest1 = AI.GetValidDest().Find(x => x != dests.First());
            if (dest1 is null) dest1 = Src;
            return new Decision { action = true, cards = cards, dests = new List<Player> { dests.First(), dest1 } };
        }
    }
}