using System.Collections.Generic;

namespace GameCore
{
    public abstract class Converted : Skill
    {
        public abstract Card Convert(List<Card> cards);

        public virtual Card Use(List<Card> cards)
        {
            Execute();
            return Convert(cards);
        }

        public override int MaxCard => 1;

        public override int MinCard => 1;

        public override bool IsValidCard(Card card) => card.useable;

        public override bool IsValid
        {
            get
            {
                var card = Convert(null);
                card.src = src;
                return base.IsValid && Timer.Instance.maxCard > 0 && Timer.Instance.isValidCard(card);
            }
        }
    }
}
