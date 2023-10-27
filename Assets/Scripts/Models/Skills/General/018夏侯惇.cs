using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 刚烈 : Triggered
{
    protected override bool OnDamaged(Damaged damaged) => damaged.Src != src && damaged.Src != null;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Player dest;

    public override async Task Invoke(object arg)
    {
        var damaged = arg as Damaged;
        dest = damaged.Src;

        for (int i = 0; i < damaged.value; i++)
        {
            var decision = await WaitDecision();
            if (!decision.action) return;

            Execute(decision);
            var judge = await new Judge().Execute();

            // 红色
            if (judge.isRed) await new Damaged(dest, src).Execute();

            // 黑色
            else
            {
                if (dest.CardCount == 0) return;
                CardPanel.Instance.Title = "刚烈";
                CardPanel.Instance.Hint = "对" + dest + "发动刚烈，弃置其一张牌";
                var cards = await TimerAction.SelectOneCard(src, dest);
                await new Discard(dest, cards).Execute();
            }
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

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        dest = decision.dests[0];
        offset = 0;

        if (decision.cards.Find(x => x.type == "基本牌") != null) offset++;
        if (decision.cards.Find(x => x.type == "锦囊牌" || x.type == "延时锦囊") != null) offset++;
        if (decision.cards.Find(x => x is Equipment) != null) offset++;
        dest.HandCardLimitOffset += offset;

        await new GetCardFromElse(dest, src, decision.cards).Execute();
    }

    private Player dest;
    private int offset;

    protected override void ResetAfterTurn()
    {
        if (dest is null) return;

        time = 0;
        dest.HandCardLimitOffset -= offset;
        dest = null;
    }

    public override Decision AIDecision()
    {
        var cards = AI.GetRandomCard();
        var dests = AI.GetDestByTeam(src.team).ToList();
        if (cards.Count == 0 || dests.Count == 0 || !AI.CertainValue) return new();

        return new Decision { action = true, cards = cards, dests = AI.Shuffle(dests) };
    }
}
