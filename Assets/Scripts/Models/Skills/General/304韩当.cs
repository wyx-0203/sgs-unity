using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

public class 弓骑 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;

    public override async Task Use(Decision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        string suit = decision.cards[0].suit;

        // 攻击范围无限
        src.effects.NoAttactRangeLimit(LifeType.UntilTurnEnd);
        // 次数无限
        src.effects.NoTimesLimit.Add(x => x is 杀 && x.suit == suit, LifeType.UntilTurnEnd);

        // 选择一名角色
        if (decision.cards[0] is not Equipment) return;
        Timer.Instance.hint = "弃置一名其他角色的一张牌";
        Timer.Instance.isValidDest = x => x != src && x.CardCount > 0;
        Timer.Instance.DefaultAI = AI.TryAction;

        decision = await Timer.Instance.Run(src, 0, 1);
        if (!decision.action) return;

        // 弃一张牌
        CardPanel.Instance.Title = "弓骑";
        CardPanel.Instance.Hint = "弃置其一张牌";
        var dest = decision.dests[0];
        var card = await TimerAction.SelectOneCard(src, dest);
        await new Discard(dest, card).Execute();
    }
}

public class 解烦 : Active, Ultimate
{
    public bool IsDone { get; set; } = false;

    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override async Task Use(Decision decision)
    {
        if (TurnSystem.Instance.Round > 1) IsDone = true;
        Execute(decision);

        var dest = decision.dests[0];

        foreach (var i in SgsMain.Instance.AlivePlayers.Where(x => x.DestInAttackRange(dest)).OrderBy(x => x.orderKey))
        {
            Timer.Instance.hint = "弃置一张武器牌，或令该角色摸一张牌";
            Timer.Instance.isValidCard = x => x is Weapon;

            decision = await Timer.Instance.Run(i, 1, 0);
            if (decision.action) await new Discard(i, decision.cards).Execute();
            else await new GetCardFromPile(dest, 1).Execute();
        }
    }

    public override Decision AIDecision()
    {
        // 随机指定一名队友
        var dests = AI.GetDestByTeam(src.team).OrderBy(x => -SgsMain.Instance.AlivePlayers.Where(y => y.DestInAttackRange(x)).Count());
        Timer.Instance.temp.dests.Add(dests.First());
        return base.AIDecision();
    }
}
