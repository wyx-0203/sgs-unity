using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 仁德 : Active
    {
        public 仁德(Player src) : base(src) { }
        public override int TimeLimit => int.MaxValue;

        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override bool IsValidCard(Card card) => Src.HandCards.Contains(card);
        public override bool IsValidDest(Player dest) => dest != Src && !invalidDest.Contains(dest);

        private List<Player> invalidDest = new List<Player>();
        private int count = 0;
        private bool done = false;

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            count += cards.Count;
            invalidDest.Add(dests[0]);
            await new GetCardFromElse(dests[0], Src, cards).Execute();
            if (count < 2 || done) return;

            done = true;
            var list = new List<Card>
            {
                Card.Convert<杀>(), Card.Convert<火杀>(), Card.Convert<雷杀>(), Card.Convert<酒>(), Card.Convert<桃>()
            };
            foreach (var i in list) Timer.Instance.MultiConvert.Add(i);
            Timer.Instance.IsValidCard = CardArea.Instance.ValidCard;
            Timer.Instance.MaxDest = DestArea.Instance.MaxDest;
            Timer.Instance.MinDest = DestArea.Instance.MinDest;
            Timer.Instance.IsValidDest = DestArea.Instance.ValidDest;
            if (!await Timer.Instance.Run(Src)) return;

            var card = list.Find(x => x.Name == Timer.Instance.Other);
            await card.UseCard(Src, Timer.Instance.Dests);
        }

        protected override void Reset()
        {
            base.Reset();
            count = 0;
            done = false;
            invalidDest.Clear();
        }
    }
}