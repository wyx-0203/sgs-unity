using System.Threading.Tasks;

namespace GameCore
{
    /// <summary>
    /// 主动技
    /// </summary>
    public abstract class Active : Skill
    {
        public override int timeLimit => 1;

        // public override bool IsValid => Timer.Instance.playRequest.type == Model.PlayRequest.Type.PlayPhase && base.IsValid;

        // public override PlayDecision AIDecision()
        // {
        //     Timer.Instance.temp.action = Timer.Instance.temp.cards.Count >= MinCard
        //         && Timer.Instance.temp.cards.Count <= MaxCard
        //         && Timer.Instance.temp.dests.Count >= MinDest
        //         && Timer.Instance.temp.dests.Count <= MaxDest;

        //     return Timer.Instance.SaveTemp();
        // }

        public abstract Task Use(PlayDecision decision);
    }
}