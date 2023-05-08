using System.Threading.Tasks;

namespace Model
{
    public class 烈弓 : Triggered
    {
        public 烈弓(Player src) : base(src) { }

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            Src.events.AfterUseCard.AddEvent(Src, Execute);
            Src.unlimitedDst += IsUnlimited;
        }

        public override void OnDisable()
        {
            Src.events.AfterUseCard.RemoveEvent(Src);
            Src.unlimitedDst -= IsUnlimited;
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀) return;
            foreach (var i in card.Dests)
            {
                if (i.HandCardCount > Src.HandCardCount && i.Hp < Src.Hp) continue;

                dest = i;

                if (!await base.ShowTimer()) continue;
                Execute();
                if (i.HandCardCount <= Src.HandCardCount) (card as 杀).ShanCount[i.position] = 0;
                if (i.Hp >= Src.Hp) (card as 杀).DamageValue[i.position]++;
            }
        }

        private Player dest;

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && card.Weight >= Src.GetDistance(dest);

        // protected override bool AIResult() => Src.team != dest.team;


        protected override bool AIResult()
        {
            bool result = dest.team != Src.team;
            if (result) AI.Instance.SelectDest();
            return result;
        }
    }
}