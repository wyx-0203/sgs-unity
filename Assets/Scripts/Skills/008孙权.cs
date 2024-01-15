using GameCore;
using System.Linq;
using System.Threading.Tasks;

public class 制衡 : Active
{
    public override int MaxCard => int.MaxValue;
    public override int MinCard => 1;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);

        int count = decision.cards.Count;
        if (decision.cards.Where(x => x.isHandCard).Count() == src.handCardsCount) count++;

        await new Discard(src, decision.cards).Execute();
        await new DrawCard(src, count).Execute();
    }

    public override PlayDecision AIDecision()
    {
        // 优先出牌
        // if (TurnSystem.Instance.PlayDecisions.Count > 0 && AI.CertainValue) return new();

        // Timer.Instance.temp.cards = AI.GetRandomCard();
        return base.AIDecision();
    }
}
