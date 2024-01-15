using GameCore;
using System.Collections.Generic;

public class 乱击 : Converted
{
    public override Card Convert(List<Card> cards) => Card.Convert<万箭齐发>(src, cards);

    public override int MaxCard => 2;
    public override int MinCard => 2;
    public override bool IsValidCard(Card card) => card.isHandCard && base.IsValidCard(card);
    public override Model.SinglePlayQuery.Type type => Model.SinglePlayQuery.Type.LuanJi;
}
