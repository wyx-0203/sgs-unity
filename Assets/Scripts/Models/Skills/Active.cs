using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 主动技
    /// </summary>
    public abstract class Active : Skill
    {
        public override int timeLimit => 1;

        public override bool IsValid => Timer.Instance.type == Timer.Type.PlayPhase && base.IsValid;

        public override Decision AIDecision()
        {
            Timer.Instance.temp.action = Timer.Instance.temp.cards.Count >= MinCard
                && Timer.Instance.temp.cards.Count <= MaxCard
                && Timer.Instance.temp.dests.Count >= MinDest
                && Timer.Instance.temp.dests.Count <= MaxDest;

            return Timer.Instance.SaveTemp();
        }

        public abstract Task Use(Decision decision);
        // {
        //     await Task.Yield();
        //     time++;
        //     // Dests = decision?.dests;
        //     this.decision = decision;
        //     if (!MCTS.Instance.isRunning) UseSkillView?.Invoke(this);
        //     string destStr = decision != null && decision.dests.Count > 0 ? "对" + string.Join("、", decision.dests) : "";
        //     Util.Print(src + destStr + "使用了" + name);
        // }
    }
}