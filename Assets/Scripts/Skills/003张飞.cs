using GameCore;

public class 咆哮 : Skill, Durative
{
    public override bool passive => true;

    public void OnStart()
    {
        src.effects.NoTimesLimit.Add(x => x is 杀, this);
        src.effects.NoDistanceLimit.Add((card, player) => card is 杀 && src.shaCount > 0, this);
    }
}
