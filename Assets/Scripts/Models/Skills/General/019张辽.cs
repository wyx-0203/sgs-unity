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

        public override bool IsValidDest(Player dest) => dest.HandCardCount > 0 && dest != Src;

        private GetCardFromPile getCardFromPile;

        public async Task Execute(GetCard getCard)
        {
            if (getCard is not GetCardFromPile getCardFromPile || !getCardFromPile.InGetCardPhase) return;

            var decision = await WaitDecision();
            if (!decision.action) return;
            TurnSystem.Instance.SortDest(decision.dests);
            await Execute(decision);

            getCardFromPile.Count -= decision.dests.Count;
            foreach (var i in decision.dests)
            {
                CardPanel.Instance.Title = "突袭";
                CardPanel.Instance.Hint = "对" + i + "号位发动突袭，获得其一张牌";

                decision = await CardPanel.Instance.Run(Src, i, i.HandCards);
                await new GetCardFromElse(Src, i, decision.cards).Execute();
            }
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetDestByTeam(!Src.team).Take(getCardFromPile.Count).ToList();
            return dests.Count > 0 ? new Decision { action = true, dests = dests } : new();
        }
    }
}