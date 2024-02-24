using GameCore;
using System;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 龙吟 : Triggered
{
    public override bool OnEveryUseCard(Card card) => card is 杀
        && src.cardsCount > 0
        && card.src == game.turnSystem.CurrentPlayer
        && game.turnSystem.CurrentPhase == Phase.Play;

    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override bool IsValidDest(Player dest) => dest == this.dest;

    private Card card => arg as Card;
    private Player dest => card.src;

    protected override async Task Invoke(PlayDecision decision)
    {
        var card = this.card;
        var dest = this.dest;

        await new Discard(src, decision.cards).Execute();
        dest.shaCount--;
        if (card.isRed) await new DrawCard(src, 1).Execute();
        if (decision.cards[0].weight == card.weight) src.FindSkill<竭忠>().IsDone = false;
    }

    public override bool AIAct => dest.team == src.team || (card.isRed && new Random().NextDouble() < 0.5f);

    // public override Decision AIDecision()
    // {
    //     if (src.cardsCount == 0 || dest.team != src.team && (!card.isRed || UnityEngine.Random.value < 0.7f)) return new();
    //     return AI.TryAction();
    // }
}

public class 竭忠 : Triggered, Limited
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Play && count > 0;
    public bool IsDone { get; set; } = false;

    private int count => src.hpLimit - src.handCardsCount;

    protected override async Task Invoke(PlayDecision decision)
    {
        IsDone = true;
        await new DrawCard(src, count).Execute();
    }

    // public override Decision AIDecision() => new Decision { action = count > 1 ^ !AI.CertainValue };
    public override bool AIAct => count > 1;
}
