using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 突袭 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.WhenGetCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.WhenGetCard.RemoveEvent(Src);
        }

        public override int MaxDest => getCardFromPile.Count;
        public override int MinDest => 1;

        public override bool IsValidDest(Player dest) => dest.HandCardCount > 0;

        private GetCardFromPile getCardFromPile;

        public async Task Execute(GetCard getCard)
        {
            getCardFromPile = getCard as GetCardFromPile;
            if (getCardFromPile is null || !getCardFromPile.InGetCardPhase) return;

            var decision = await WaitDecision();
            if (!decision.action) return;
            TurnSystem.Instance.SortDest(decision.dests);
            await Execute(decision);

            getCardFromPile.Count -= decision.dests.Count;
            foreach (var i in decision.dests)
            {
                CardPanel.Instance.Title = "突袭";
                CardPanel.Instance.Hint = "对" + i.posStr + "号位发动突袭，获得其一张牌";

                decision = await CardPanel.Instance.Run(Src, i, i.HandCards);
                var card = decision.action ? decision.cards : new List<Card> { i.HandCards[0] };
                await new GetCardFromElse(Src, i, card).Execute();
            }
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetDestByTeam(!Src.team).Take(getCardFromPile.Count).ToList();
            return dests.Count > 0 ? new Decision { action = true, dests = dests } : new();
        }
    }
}