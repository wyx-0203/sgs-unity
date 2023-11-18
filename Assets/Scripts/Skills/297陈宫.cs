using GameCore;
using System.Linq;
using System.Threading.Tasks;

public class 明策 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override int MaxDest => 2;
    public override int MinDest => 2;

    public override bool IsValidCard(Card card) => !card.isConvert && (card is 杀 || card is Equipment);
    public override bool IsValidDest(Player dest) => firstDest is null ? dest != src : firstDest.DestInAttackRange(dest);

    public override async Task Use(Decision decision)
    {
        Execute(decision);
        var dest = decision.dests[0];

        await new GetCardFromElse(dest, src, decision.cards).Execute();

        Timer.Instance.hint = "视为对该角色使用一张杀，或摸一张牌";
        Timer.Instance.isValidDest = dest => dest == decision.dests[1];

        // 若阵营不同，则选择出杀
        Timer.Instance.DefaultAI = dest.team != decision.dests[1].team || !AI.CertainValue ? AI.TryAction : () => new();

        decision = await Timer.Instance.Run(dest, 0, 1);
        if (decision.action) await Card.Convert<杀>(dest).UseCard(dest, decision.dests);
        else await new GetCardFromPile(dest, 1).Execute();
    }

    public override Decision AIDecision()
    {
        var cards = AI.GetRandomCard();
        var dests = AI.GetDestByTeam(src.team).ToList();
        if (cards.Count == 0 || dests.Count == 0) return new();

        Timer.Instance.temp.cards = cards;
        Timer.Instance.temp.dests.Add(dests[0]);

        var dest1 = AI.GetValidDest().Find(x => x != dests[0]);
        if (dest1 != null) Timer.Instance.temp.dests.Add(dest1);
        return base.AIDecision();
    }
}

public class 智迟 : Triggered
{
    public override bool isObey => true;
    protected override bool OnDamaged(Damaged damaged) => TurnSystem.Instance.CurrentPlayer != src;

    protected override async Task Invoke(Decision decision)
    {
        await Task.Yield();
        src.effects.InvalidForDest.Add(x => x is 杀 || x is Scheme, Duration.UntilTurnEnd);
    }
}
