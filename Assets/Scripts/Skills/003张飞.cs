using GameCore;

public class 咆哮 : Skill, Durative
{
    public override bool isObey => true;

    public void OnStart()
    {
        src.effects.NoTimesLimit.Add(x => x is 杀, this);
    }
}
