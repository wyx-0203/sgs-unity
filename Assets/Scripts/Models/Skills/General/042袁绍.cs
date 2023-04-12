using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class 乱击 : Converted
    {
        public 乱击(Player src) : base(src) { }
        public override string CardName => "万箭齐发";

        public override Card Execute(List<Card> cards) => Card.Convert<万箭齐发>(cards);

        public override int MaxCard => 2;
        public override int MinCard => 2;

        public override bool IsValidCard(Card card) => (first is null || first.Suit == card.Suit)
            && base.IsValidCard(card);
        private Card first => Operation.Instance.Cards.Count > 0 ? Operation.Instance.Cards[0] : null;
    }
}