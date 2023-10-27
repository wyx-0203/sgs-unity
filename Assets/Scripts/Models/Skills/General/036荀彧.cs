using Model;
using System.Linq;
using System.Threading.Tasks;

public class 驱虎 : Active
{
    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => dest.Hp > src.Hp && dest.HandCardCount > 0;

    public override bool IsValid => base.IsValid && src.HandCardCount > 0;

    public override async Task Use(Decision decision)
    {
        Execute(decision);
        var dest = decision.dests[0];
        var compete = await TimerAction.Compete(src, dest);

        // 拼点没赢
        if (compete[0].weight <= compete[1].weight)
        {
            await new Damaged(src, decision.dests[0]).Execute();
            return;
        }

        // 攻击范围内没有角色，直接返回
        if (SgsMain.Instance.AlivePlayers.Find(x => dest.DestInAttackRange(x)) is null) return;

        Timer.Instance.hint = "请选择一名角色";
        Timer.Instance.refusable = false;
        Timer.Instance.isValidDest = x => dest.DestInAttackRange(x);
        Timer.Instance.DefaultAI = AI.TryAction;

        decision = await Timer.Instance.Run(src, 0, 1);
        await new Damaged(decision.dests[0], dest).Execute();
    }

    public override Decision AIDecision()
    {
        var dests = AI.GetValidDest();
        Timer.Instance.temp.dests.AddRange(dests.Take(1));
        return base.AIDecision();
    }
}

public class 节命 : Triggered
{
    protected override bool OnDamaged(Damaged damaged) => true;

    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override async Task Invoke(object arg)
    {
        int value = (arg as Damaged).value;
        for (int i = 0; i < value; i++)
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            Execute(decision);

            var dest = decision.dests[0];
            int count = dest.HpLimit - dest.HandCardCount;
            if (count > 0) await new GetCardFromPile(dest, count).Execute();
        }
    }

    public override Decision AIDecision()
    {
        var dests = AI.GetDestByTeam(src.team).OrderBy(x => x.HandCardCount).Take(1);
        return dests.Count() > 0 ? new Decision { action = true, dests = dests.ToList() } : new();
    }
}
