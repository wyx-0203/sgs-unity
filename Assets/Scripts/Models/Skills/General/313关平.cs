using Model;
using System;
using System.Threading.Tasks;

public class 龙吟 : Triggered
{
    public override bool OnEveryUseCard(Card card) => card is 杀 && card.Src == TurnSystem.Instance.CurrentPlayer;
    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override bool IsValidDest(Player dest) => dest == this.dest;

    private Card card;
    private Player dest;

    public override async Task Invoke(object arg)
    {
        card = arg as Card;
        dest = card.Src;

        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        dest.shaCount--;
        if (card.isRed) await new GetCardFromPile(src, 1).Execute();
        if (decision.cards[0].weight == card.weight) (src.skills.Find(x => x is 竭忠) as 竭忠).IsDone = false;
    }


    public override Decision AIDecision()
    {
        if (src.CardCount == 0 || dest.team != src.team && (!card.isRed || UnityEngine.Random.value < 0.7f)) return new();
        return AI.TryAction();
    }
}

public class 竭忠 : Triggered, Ultimate
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Play && count > 0;

    public bool IsDone { get; set; } = false;

    private int count => src.HpLimit - src.HandCardCount;

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute();
        IsDone = true;
        await new GetCardFromPile(src, count).Execute();
    }

    public override Decision AIDecision() => new Decision { action = count > 1 };
}
