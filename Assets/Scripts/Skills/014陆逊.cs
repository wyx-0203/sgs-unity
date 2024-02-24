using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore;

public class 谦逊 : Triggered
{
    public override bool AfterEveryUseCard(Card card) => card is Scheme
        && card.dests.Count == 1
        && card.dest == src
        && src.handCardsCount > 0;

    // private List<Card> cards;

    protected override async Task Invoke(PlayDecision decision)
    {
        var cards = src.handCards.ToList();
        await new LoseCard(src, cards).Execute();
        game.turnSystem.AfterTurnAsyncTask += () => new GetCard(src, cards).Execute();
    }
}

public class 连营 : Triggered
{
    protected override bool OnLoseCard(LoseCard loseCard) => loseCard.HandCards.Count > 0 && src.handCardsCount == 0;

    public override int MinDest => 1;
    public override int MaxDest => count;
    private int count => (arg as LoseCard).HandCards.Count;

    protected override async Task Invoke(PlayDecision decision)
    {
        decision.dests.Sort();
        foreach (var i in decision.dests)
        {
            await new DrawCard(i, 1).Execute();
        }
    }

    public override PlayDecision AIDecision() => new()
    {
        // 优先对自己和队友使用
        dests = AIGetDestsByTeam(src.team).OrderBy(x => x != src).Take(MaxDest).ToList()
    };
}