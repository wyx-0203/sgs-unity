using GameCore;

public class 咆哮 : Skill, Durative
{
    public override bool passive => true;

    public void OnStart()
    {
        src.effects.NoTimesLimit.Add(x => x is 杀, this);
    }
}
