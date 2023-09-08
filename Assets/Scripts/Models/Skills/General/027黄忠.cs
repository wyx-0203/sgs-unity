using System.Threading.Tasks;

namespace Model
{
    public class 烈弓 : Triggered
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            Src.events.AfterUseCard.AddEvent(Src, Execute);
            Src.unlimitDst += IsUnlimited;
        }

        public override void OnDisable()
        {
            Src.events.AfterUseCard.RemoveEvent(Src);
            Src.unlimitDst -= IsUnlimited;
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀 sha) return;
            foreach (var i in card.Dests)
            {
                if (i.HandCardCount > Src.HandCardCount && i.Hp < Src.Hp) continue;
                dest = i;

                var decision = await WaitDecision();
                if (!decision.action) continue;
                await Execute(decision);

                if (i.HandCardCount <= Src.HandCardCount) sha.ShanCount[i.position] = 0;
                if (i.Hp >= Src.Hp) sha.DamageValue[i.position]++;
            }
        }

        private Player dest;

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && card.weight >= Src.GetDistance(dest);

        public override Decision AIDecision() => dest.team != Src.team || !AI.CertainValue ? AI.AutoDecision() : new();

    }
}