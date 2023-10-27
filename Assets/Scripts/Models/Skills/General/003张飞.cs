using Model;

public class 咆哮 : Triggered
{
    public override bool isObey => true;

    protected override void Init(string name, Player src)
    {
        base.Init(name, src);
        src.effects.NoTimesLimit.Add(x => x is 杀, this);
    }
}
