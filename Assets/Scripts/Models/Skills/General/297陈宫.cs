using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 明策 : Active
    {
        public 明策(Player src) : base(src) { }

        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidCard(Card card) => !card.IsConvert && (card is 杀 || card is Equipage);
        public override bool IsValidDest(Player dest) => firstDest is null || DestArea.Instance.UseSha(firstDest, dest);

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            await base.Execute(dests, cards, other);

            await new GetCardFromElse(dests[0], Src, cards).Execute();

            Timer.Instance.Hint = "视为对该角色使用一张杀，或摸一张牌";
            Timer.Instance.IsValidDest = dest => dest == dests[1];
            bool result = await Timer.Instance.Run(dests[0], 0, 1);

            if (result) await Card.Convert<杀>().UseCard(dests[0], Timer.Instance.Dests);
            else await new GetCardFromPile(dests[0], 1).Execute();
        }
    }

    public class 智迟 : Triggered
    {
        public 智迟(Player src) : base(src) { }
        public override bool Passive => true;

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
            if (TurnSystem.Instance.CurrentPlayer == Src) return;

            Execute();
            Src.disableForMe += Disable;
            TurnSystem.Instance.AfterTurn += Reset;
            await Task.Yield();
        }

        public bool Disable(Card card)
        {
            Execute();
            return card is 杀 || card.Type == "锦囊牌";
        }
        protected override void Reset()
        {
            Src.disableForMe -= Disable;
            TurnSystem.Instance.AfterTurn -= Reset;
        }
    }
}