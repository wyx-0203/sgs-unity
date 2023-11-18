using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 刚烈 : Triggered
{
    protected override bool OnDamagedByElse(Damaged damaged) => true;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Damaged damaged => arg as Damaged;
    private Player dest => damaged.Src;

    protected override async Task Invoke(Decision decision)
    {
        var judge = await Judge.Execute();

        // 红色
        if (judge.isRed) await new Damaged(dest, src).Execute();

        // 黑色
        else
        {
            if (dest.cardsCount == 0) return;
            CardPanel.Instance.Title = "刚烈";
            CardPanel.Instance.Hint = "对" + dest + "发动刚烈，弃置其一张牌";
            var cards = await TimerAction.SelectOneCardFromElse(src, dest);
            await new Discard(dest, cards).Execute();
        }
    }

    public override Decision AIDecision() => dest.team != src.team || !AI.CertainValue ? AI.TryAction() : new();
}
public class 清俭 : Triggered
{
    public override int timeLimit => 1;

    protected override bool OnGetCard(GetCard getCard) => getCard is not GetCardFromPile getCardFromPile || !getCardFromPile.inGetPhase;

    public override int MaxCard => int.MaxValue;
    public override int MinCard => 1;
    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override bool IsValidDest(Player dest) => dest != src;

    protected override async Task Invoke(Decision decision)
    {
        var dest = decision.dests[0];
        int offset = 0;

        if (decision.cards.Find(x => x is BasicCard) != null) offset++;
        if (decision.cards.Find(x => x is Scheme || x is DelayScheme) != null) offset++;
        if (decision.cards.Find(x => x is Equipment) != null) offset++;
        dest.HandCardLimitOffset += offset;

        await new GetCardFromElse(dest, src, decision.cards).Execute();

        TurnSystem.Instance.AfterTurn += () =>
        {
            time = 0;
            dest.HandCardLimitOffset -= offset;
        };
    }

    public override Decision AIDecision()
    {
        var cards = AI.GetRandomCard();
        var dests = AI.GetDestByTeam(src.team).ToList();
        if (cards.Count == 0 || dests.Count == 0 || !AI.CertainValue) return new();

        return new Decision { action = true, cards = cards, dests = AI.Shuffle(dests) };
    }
}
