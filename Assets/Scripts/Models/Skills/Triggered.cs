using System.Threading.Tasks;

namespace Model
{
    public class Triggered : Skill
    {
        // public Triggered(Player src) : base(src) { }

        /// <summary>
        /// 询问是否发动技能
        /// </summary>
        protected async Task<Decision> WaitDecision()
        {
            // Timer.Instance.givenSkill = Name;
            Timer.Instance.temp.skill = this;
            Timer.Instance.hint = "是否发动" + Name + "？";
            Timer.Instance.DefaultAI = AIDecision;
            return await Timer.Instance.Run(Src);
        }

        public override Decision AIDecision() => AI.TryAction();
    }
}
