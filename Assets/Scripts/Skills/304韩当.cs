using GameCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class 弓骑 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        string suit = decision.cards[0].suit;

        // 攻击范围无限
        src.effects.NoAttactRangeLimit(Duration.UntilTurnEnd);
        // 次数无限
        src.effects.NoTimesLimit.Add(x => x is 杀 && x.suit == suit, Duration.UntilTurnEnd);

        // 选择一名角色
        if (decision.cards[0] is not Equipment) return;
        // Timer.Instance.hint = "弃置一名其他角色的一张牌";
        // Timer.Instance.isValidDest = x => x != src && x.cardsCount > 0;
        // Timer.Instance.defaultAI = AI.TryAction;

        decision = await new PlayQuery
        {
            player = src,
            hint = "弃置一名其他角色的一张牌",
            isValidDest = player => player != src && player.cardsCount > 0,
        }.Run(0, 1);
        if (!decision.action) return;

        // 弃一张牌
        // CardPanelRequest.Instance.title = "弓骑";
        var dest = decision.dests[0];
        string hint = "弃置其一张牌";
        // var card = await TimerAction.SelectCardFromElse(src, dest);
        var cards = await new CardPanelQuery(src, dest, name, hint, false).Run();
        await new Discard(dest, cards).Execute();
    }
}

public class 解烦 : Active, Limited
{
    public bool IsDone { get; set; } = false;

    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override async Task Use(PlayDecision decision)
    {
        IsDone = true;
        Execute(decision);
        var dest = decision.dests[0];

        foreach (var i in game.AlivePlayers.Where(x => x.DestInAttackRange(dest)).OrderBy(x => x.orderKey))
        {
            // Timer.Instance.hint = "弃置一张武器牌，或令该角色摸一张牌";
            // Timer.Instance.isValidCard = x => x is Weapon;

            decision = await new PlayQuery
            {
                player = i,
                hint = $"弃置一张武器牌，或令{dest}摸一张牌",
                isValidCard = x => x is Weapon
            }.Run(1, 0);
            if (decision.action) await new Discard(i, decision.cards).Execute();
            else await new DrawCard(dest, 1).Execute();
        }

        if (game.turnSystem.round == 1) game.turnSystem.AfterTurn += () => IsDone = false;
    }

    public override PlayDecision AIDecision() => AIUseToTeammate();
}
