using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 弓骑 : Active
    {
        public 弓骑(Player src) : base(src) { }

        public override int MaxCard => 1;
        public override int MinCard => 1;

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            await base.Execute(dests, cards, other);

            await new Discard(Src, cards).Execute();
            Src.AttackRange += 10;
            suit = cards[0].Suit;
            Src.unlimitedCard += UnlimitedCard;
            TurnSystem.Instance.AfterTurn += Reset1;
            if (cards[0] is not Equipage) return;

            Timer.Instance.Hint = "弃置一名其他角色的一张牌";
            Timer.Instance.IsValidDest = x => x != Src;
            if (await Timer.Instance.Run(Src, 0, 1))
            {
                CardPanel.Instance.Title = "弓骑";
                CardPanel.Instance.Hint = "弃置其一张牌";
                var dest = Timer.Instance.dests[0];
                var card = await CardPanel.Instance.SelectCard(Src, dest);
                await new Discard(dest, new List<Card> { card }).Execute();
            }
        }

        private string suit;

        public bool UnlimitedCard(Card card) => card is 杀 && card.Suit == suit;

        public void Reset1()
        {
            Src.AttackRange -= 10;
            Src.unlimitedCard -= UnlimitedCard;
            TurnSystem.Instance.AfterTurn -= Reset1;
        }
    }

    public class 解烦 : Active, Ultimate
    {
        public 解烦(Player src) : base(src) { }
        public bool IsDone { get; set; } = false;

        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            if (TurnSystem.Instance.Round > 1) IsDone = true;
            base.Execute();

            var dest = dests[0];
            Player i = TurnSystem.Instance.CurrentPlayer;
            while (true)
            {
                if (i.AttackRange >= i.GetDistance(dest) && i != dest)
                {
                    Timer.Instance.Hint = "弃置一张武器牌，或令该角色摸一张牌";
                    Timer.Instance.IsValidCard = x => x is Weapon;
                    bool result = await Timer.Instance.Run(i, 1, 0);
                    if (result) await new Discard(i, Timer.Instance.cards).Execute();
                    else await new GetCardFromPile(dest, 1).Execute();
                }
                i = i.next;
                if (i == TurnSystem.Instance.CurrentPlayer) break;
            }
        }
    }
}