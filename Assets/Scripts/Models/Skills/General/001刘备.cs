using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 仁德 : Active
    {
        public override int TimeLimit => int.MaxValue;

        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override bool IsValidCard(Card card) => card.IsHandCard;
        public override bool IsValidDest(Player dest) => dest != Src && !invalidDest.Contains(dest);

        private List<Player> invalidDest = new();
        private int count = 0;
        private bool done = false;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);

            count += decision.cards.Count;
            invalidDest.Add(decision.dests[0]);
            await new GetCardFromElse(decision.dests[0], Src, decision.cards).Execute();
            if (count < 2 || done) return;

            done = true;
            var list = new List<Card> { Card.Convert<杀>(), Card.Convert<火杀>(), Card.Convert<雷杀>(), Card.Convert<酒>(), Card.Convert<桃>() };

            Timer.Instance.multiConvert.AddRange(list);
            Timer.Instance.isValidCard = CardArea.Instance.ValidCard;
            Timer.Instance.maxDest = DestArea.Instance.MaxDest;
            Timer.Instance.minDest = DestArea.Instance.MinDest;
            Timer.Instance.isValidDest = DestArea.Instance.ValidDest;
            Timer.Instance.DefaultAI = () =>
            {
                List<Decision> decisions = new();
                var validCards = Timer.Instance.multiConvert.Where(x => Timer.Instance.isValidCard(x)).ToList();
                foreach (var i in validCards)
                {
                    Timer.Instance.temp.converted = i;

                    if (Timer.Instance.maxDest() > 0)
                    {
                        var dests = AI.GetValidDest();
                        if (dests is null || dests[0].team == Src.team) continue;

                        Timer.Instance.temp.dests.AddRange(dests);
                    }

                    Timer.Instance.temp.action = true;
                    decisions.Add(Timer.Instance.SaveTemp());
                }

                if (decisions.Count == 0 || !AI.CertainValue) decisions.Add(new Decision());
                return AI.Shuffle(decisions)[0];
            };

            decision = await Timer.Instance.Run(Src);
            if (!decision.action) return;

            await decision.converted.UseCard(Src, decision.dests);
        }

        protected override void ResetAfterPlay()
        {
            base.ResetAfterPlay();
            count = 0;
            done = false;
            invalidDest.Clear();
        }

        public override Decision AIDecision()
        {
            Timer.Instance.temp.dests = AI.GetDestByTeam(Src.team).Take(1).ToList();
            Timer.Instance.temp.cards = AI.GetRandomCard();
            return base.AIDecision();
        }
    }
}