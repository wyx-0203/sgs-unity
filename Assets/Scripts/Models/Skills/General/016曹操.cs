using Model;
using System.Threading.Tasks;

public class 奸雄 : Triggered
{
    protected override bool OnDamaged(Damaged damaged) => true;

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;

        Execute(decision);
        var srcCard = (arg as Damaged).SrcCard;

        // 若伤害来源牌在弃牌堆
        if (srcCard != null)
        {
            var cards = srcCard.InDiscardPile();
            await new GetDisCard(src, cards).Execute();
        }

        // 摸一张牌
        await new GetCardFromPile(src, 1).Execute();
    }
}
