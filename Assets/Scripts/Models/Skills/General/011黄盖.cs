using Model;
using System.Threading.Tasks;

public class 苦肉 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;

    public override async Task Use(Decision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        await new UpdateHp(src, -1).Execute();
    }

    public override Decision AIDecision()
    {
        if (src.Hp < 2) return new();
        Timer.Instance.temp.cards = AI.GetRandomCard();
        return base.AIDecision();
    }
}

public class 诈降 : Triggered
{
    public override bool isObey => true;

    protected override bool OnLoseHp(UpdateHp updateHp) => true;

    public override async Task Invoke(object arg)
    {
        Execute();
        int value = -(arg as UpdateHp).value;

        // 摸三张牌
        await new GetCardFromPile(src, 3 * value).Execute();
        if (TurnSystem.Instance.CurrentPlayer != src || TurnSystem.Instance.CurrentPhase != Phase.Play) return;

        // 出杀次数加1
        src.shaCount -= value;
        // 红杀无距离限制
        src.effects.NoDistanceLimit.Add(x => x.Item1 is 杀 && x.Item1.isRed, LifeType.UntilPlayPhaseEnd);
        // 红杀不可闪避
        src.effects.Unmissable.Add(x => x.Item1 is 杀 && x.Item1.isRed, LifeType.UntilPlayPhaseEnd);
    }
}
