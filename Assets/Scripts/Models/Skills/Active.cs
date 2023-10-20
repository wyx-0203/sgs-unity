using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 主动技
    /// </summary>
    public class Active : Skill
    {
        public override int TimeLimit => 1;

        public override bool IsValid => Timer.Instance.type == Timer.Type.PlayPhase && base.IsValid;

        protected override void ResetAfterPlay()
        {
            Time = 0;
        }

        public override Decision AIDecision()
        {
            Timer.Instance.temp.action = Timer.Instance.temp.cards.Count >= MinCard
                && Timer.Instance.temp.cards.Count <= MaxCard
                && Timer.Instance.temp.dests.Count >= MinDest
                && Timer.Instance.temp.dests.Count <= MaxDest;

            return Timer.Instance.SaveTemp();
        }
    }
}