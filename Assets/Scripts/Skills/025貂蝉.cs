using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;
using Gender = Model.Gender;

public class 离间 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override int MaxDest => 2;
    public override int MinDest => 2;

    public override bool IsValidDest(Player dest) => dest != src && dest.general.gender == Gender.Male;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        await Card.Convert<决斗>(decision.dests[1]).UseCard(decision.dests[1], new List<Player> { decision.dests[0] });
    }

    public override PlayDecision AIDecision()
    {
        // var dests = AI.GetValidDest();
        // if (dests.Count < 2 || dests[0].team == src.team) return new();

        // Timer.Instance.temp.cards = AI.GetRandomCard();
        // Timer.Instance.temp.dests = dests.GetRange(0, 2);
        var players = AIGetAllDests().OrderBy(x => x.team == src.team ? 1 : 0);
        var dest0 = players.FirstOrDefault();
        // var dest1=players.FirstOrDefault(x=>x)
        if (players.Count() < 2 || dest0.team == src.team) return new();
        return new PlayDecision { dests = players.Take(2).ToList() };
    }
}

public class 闭月 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.End;

    protected override async Task Invoke(PlayDecision decision)
    {
        await new DrawCard(src, src.handCardsCount == 0 ? 2 : 1).Execute();
    }
}
