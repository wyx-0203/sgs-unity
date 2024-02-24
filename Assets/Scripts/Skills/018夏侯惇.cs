using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 刚烈 : Triggered
{
    protected override bool OnDamagedByElse(Damage damaged) => true;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Damage damaged => arg as Damage;
    private Player dest => damaged.Src;

    protected override async Task Invoke(PlayDecision decision)
    {
        var judge = await Judge.Execute(src);

        // 红色
        if (judge.isRed) await new Damage(dest, src).Execute();

        // 黑色
        else
        {
            if (dest.cardsCount == 0) return;
            // CardPanelRequest.Instance.title = "刚烈";
            string hint = $"对{dest}发动刚烈，弃置其一张牌";
            // var cards = await TimerAction.SelectCardFromElse(src, dest);
            var cards = await new CardPanelQuery(src, dest, name, hint, false).Run();
            await new Discard(dest, cards).Execute();
        }
    }

    // public override Decision AIDecision() => dest.team != src.team || !AI.CertainValue ? AI.TryAction() : new();
    public override bool AIAct => dest.team != src.team;
}
public class 清俭 : Triggered
{
    public override int timeLimit => 1;

    protected override bool OnGetCard(GetCard getCard) => (getCard is not DrawCard drawCard || !drawCard.inGetPhase)
        && game.turnSystem.round > 0;

    public override int MaxCard => src.cardsCount;
    public override int MinCard => 1;
    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override bool IsValidDest(Player dest) => dest != src;

    protected override async Task Invoke(PlayDecision decision)
    {
        var dest = decision.dests[0];
        int offset = 0;

        if (decision.cards.Find(x => x is BasicCard) != null) offset++;
        if (decision.cards.Find(x => x is Scheme || x is DelayScheme) != null) offset++;
        if (decision.cards.Find(x => x is Equipment) != null) offset++;
        dest.HandCardLimitOffset += offset;

        await new GetAnothersCard(dest, src, decision.cards).Execute();

        game.turnSystem.AfterTurn += () =>
        {
            time = 0;
            dest.HandCardLimitOffset -= offset;
        };
    }

    public override PlayDecision AIDecision() => new PlayDecision
    {
        action = true,
        cards = AIGetCards(),
        dests = game.AlivePlayers.Where(x => IsValidDest(x) && x.team == src.team).Take(1).ToList()
    };
    // {
    //     var cards = AI.GetRandomCard();
    //     var dests = AI.GetDestByTeam(src).ToList();
    //     if (cards.Count == 0 || dests.Count == 0 || !AI.CertainValue) return new();

    //     return new Decision { action = true, cards = cards, dests = AI.Shuffle(dests) };
    // }
}
