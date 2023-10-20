using System.Collections.Generic;

namespace Model
{
    public class 乱击 : Converted
    {
        public override Card Convert(List<Card> cards) => Card.Convert<万箭齐发>(cards);

        public override int MaxCard => 2;
        public override int MinCard => 2;

        public override bool IsValidCard(Card card) => (first is null || first.suit == card.suit) && card.IsHandCard && base.IsValidCard(card);
        private Card first => Timer.Instance.temp.cards.Count > 0 ? Timer.Instance.temp.cards[0] : null;
    }
}