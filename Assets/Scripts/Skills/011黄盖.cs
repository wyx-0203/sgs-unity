using GameCore;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 苦肉 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        await new LoseHp(src, 1).Execute();
    }

    public override bool AIAct => src.hp >= 2;
}

public class 诈降 : Triggered
{
    public override bool passive => true;

    protected override bool OnLoseHp(LoseHp loseHp) => true;

    protected override async Task Invoke(PlayDecision decision)
    {
        int value = (arg as LoseHp).value;

        // 摸三张牌
        await new DrawCard(src, 3 * value).Execute();
        if (game.turnSystem.CurrentPlayer != src || game.turnSystem.CurrentPhase != Phase.Play) return;

        // 出杀次数加1
        src.shaCount -= value;
        // 红杀无距离限制
        src.effects.NoDistanceLimit.Add(x => x.Item1 is 杀 && x.Item1.isRed, Duration.UntilPlayPhaseEnd);
        // 红杀不可闪避
        src.effects.Unmissable.Add(x => x.Item1 is 杀 && x.Item1.isRed, Duration.UntilPlayPhaseEnd);
    }
}
