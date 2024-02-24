using GameCore;
using System.Linq;
using System.Threading.Tasks;

public class 驱虎 : Active
{
    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => dest.hp > src.hp && dest.handCardsCount > 0;

    public override bool IsValid => base.IsValid && src.handCardsCount > 0;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);
        var dest = decision.dests[0];
        var compete = await TimerAction.Compete(src, dest);

        // 拼点没赢
        if (compete[0].weight <= compete[1].weight)
        {
            await new Damage(src, decision.dests[0]).Execute();
            return;
        }

        // 攻击范围内没有角色，直接返回
        if (game.AlivePlayers.Find(x => dest.DestInAttackRange(x)) is null) return;

        // Timer.Instance.hint = "请选择一名角色";
        // Timer.Instance.refusable = false;
        // Timer.Instance.isValidDest = x => dest.DestInAttackRange(x);
        // Timer.Instance.defaultAI = AI.TryAction;

        decision = await new PlayQuery
        {
            player = src,
            hint = "请选择一名角色",
            refusable = false,
            isValidDest = player => dest.DestInAttackRange(player),
            // defaultAI = AI.TryAction,
        }.Run(0, 1);
        await new Damage(decision.dests[0], dest).Execute();
    }

    // public override PlayDecision AIDecision()
    // {
    //     // var dests = AI.GetValidDest();
    //     // Timer.Instance.temp.dests.AddRange(dests.Take(1));
    //     return base.AIDecision();
    // }
}

public class 节命 : Triggered
{
    protected override bool OnDamaged(Damage damaged) => true;

    public override int MaxDest => 1;
    public override int MinDest => 1;

    protected override async Task Invoke(PlayDecision decision)
    {
        var dest = decision.dests[0];
        await new DrawCard(dest, dest.hpLimit).Execute();
        int count = dest.handCardsCount - dest.hpLimit;
        if (count > 0) await TimerAction.DiscardFromHand(dest, count);
    }

    public override PlayDecision AIDecision() => new PlayDecision
    {
        action = true,
        dests = game.AlivePlayers.Where(x => x.team == src.team)
            .OrderBy(x => x.handCardsCount)
            .Take(1)
            .ToList()
    };
}
