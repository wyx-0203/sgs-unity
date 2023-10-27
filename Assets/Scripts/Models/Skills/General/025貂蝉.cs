using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 离间 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override int MaxDest => 2;
    public override int MinDest => 2;

    public override bool IsValidDest(Player dest) => dest != src && dest.general.gender;

    public override async Task Use(Decision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        await Card.Convert<决斗>().UseCard(decision.dests[1], new List<Player> { decision.dests[0] });
    }

    public override Decision AIDecision()
    {
        var dests = AI.GetValidDest();
        if (dests.Count < 2 || dests[0].team == src.team) return new();

        Timer.Instance.temp.cards = AI.GetRandomCard();
        Timer.Instance.temp.dests = dests.GetRange(0, 2);
        return base.AIDecision();
    }
}

public class 闭月 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.End;

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);
        await new GetCardFromPile(src, src.HandCardCount == 0 ? 2 : 1).Execute();
    }
}
