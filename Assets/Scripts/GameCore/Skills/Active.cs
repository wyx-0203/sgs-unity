using System.Threading.Tasks;

namespace GameCore
{
    /// <summary>
    /// 主动技
    /// </summary>
    public abstract class Active : Skill
    {
        public override int timeLimit => 1;

        public override bool IsValid => Timer.Instance.type == Timer.Type.InPlayPhase && base.IsValid;

        public override Decision AIDecision()
        {
            Timer.Instance.temp.action = Timer.Instance.temp.cards.Count >= MinCard
                && Timer.Instance.temp.cards.Count <= MaxCard
                && Timer.Instance.temp.dests.Count >= MinDest
                && Timer.Instance.temp.dests.Count <= MaxDest;

            return Timer.Instance.SaveTemp();
        }

        public abstract Task Use(Decision decision);
    }
}