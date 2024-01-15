using GameCore;
using System.Threading.Tasks;

public class 奸雄 : Triggered
{
    protected override bool OnDamaged(Damage damaged) => true;

    protected override async Task Invoke(PlayDecision decision)
    {
        var srcCard = (arg as Damage).SrcCard;

        // 若伤害来源牌在弃牌堆
        if (srcCard != null)
        {
            var cards = srcCard.InDiscardPile();
            await new GetDiscard(src, cards).Execute();
        }

        // 摸一张牌
        await new DrawCard(src, 1).Execute();
    }
}
