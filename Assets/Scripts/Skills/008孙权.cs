using GameCore;
using System.Linq;
using System.Threading.Tasks;

public class 制衡 : Active
{
    public override int MaxCard => int.MaxValue;
    public override int MinCard => 1;

    public override async Task Use(Decision decision)
    {
        Execute(decision);

        int count = decision.cards.Count;
        if (decision.cards.Where(x => x.isHandCard).Count() == src.handCardsCount) count++;

        await new Discard(src, decision.cards).Execute();
        await new GetCardFromPile(src, count).Execute();
    }

    public override Decision AIDecision()
    {
        // 优先出牌
        if (TurnSystem.Instance.PlayDecisions.Count > 0 && AI.CertainValue) return new();

        Timer.Instance.temp.cards = AI.GetRandomCard();
        return base.AIDecision();
    }
}
