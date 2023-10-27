using Model;
using System.Threading.Tasks;

public class 烈弓 : Triggered
{
    protected override bool AfterSetCardTarget(Card card) => card is 杀
        && (card.dest.HandCardCount <= src.HandCardCount || card.dest.Hp >= src.Hp);

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest1) => dest1 == dest;

    private Player dest;

    public override async Task Invoke(object arg)
    {
        var sha = arg as 杀;
        dest = sha.dest;

        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        // 不可闪避
        if (dest.HandCardCount <= src.HandCardCount) sha.shanCount = 0;
        // 伤害+1
        if (dest.Hp >= src.Hp) sha.AddDamageValue(dest, 1);
    }

    public override Decision AIDecision() => dest.team != src.team || !AI.CertainValue ? AI.TryAction() : new();

    // 无视距离
    protected override void Init(string name, Player src)
    {
        base.Init(name, src);
        src.effects.NoDistanceLimit.Add((x, y) => x is 杀 && x.weight >= src.GetDistance(y), this);
    }
}
