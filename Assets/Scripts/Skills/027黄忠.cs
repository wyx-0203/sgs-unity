using GameCore;
using System.Threading.Tasks;

public class 烈弓 : Triggered, Durative
{
    protected override bool AfterUseCard(Card card) => card is 杀
        && (card.dest.handCardsCount <= src.handCardsCount || card.dest.hp >= src.hp);

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private 杀 sha => arg as 杀;
    private Player dest => sha.dest;

    protected override Task Invoke(PlayDecision decision)
    {
        // 不可闪避
        if (dest.handCardsCount <= src.handCardsCount) sha.unmissableDests.Add(dest);
        // 伤害+1
        if (dest.hp >= src.hp) sha.AddDamageValue(dest, 1);
        return Task.CompletedTask;
    }

    public override bool AIAct => dest.team != src.team;
    // public override Decision AIDecision() => dest.team != src.team || !AI.CertainValue ? AI.TryAction() : new();

    // 无视距离
    public void OnStart()
    {
        src.effects.NoDistanceLimit.Add((x, y) => x is 杀 && x.weight >= src.GetDistance(y), this);
    }
}
