using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 离间 : Active
    {
        public 离间(Player src) : base(src) { }

        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidDest(Player dest) => dest != Src && dest.general.gender;

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            await new Discard(Src, cards).Execute();
            await Card.Convert<决斗>().UseCard(dests[1], new List<Player> { dests[0] });
        }
    }

    public class 闭月 : Triggered
    {
        public 闭月(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.End].AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
        }

        public new async Task Execute()
        {
            if (!await base.ShowTimer()) return;
            base.Execute();
            await new GetCardFromPile(Src, Src.HandCardCount == 0 ? 2 : 1).Execute();
        }
    }
}