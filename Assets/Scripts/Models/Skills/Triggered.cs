using System.Threading.Tasks;

namespace Model
{
    public class Triggered : Skill
    {
        // public Triggered(Player src) : base(src) { }

        /// <summary>
        /// 询问是否发动技能
        /// </summary>
        public async Task<Decision> WaitDecision()
        {
            // Timer.Instance.givenSkill = Name;
            Timer.Instance.temp.skill = this;
            Timer.Instance.hint = "是否发动" + name + "？";
            Timer.Instance.DefaultAI = AIDecision;
            return await Timer.Instance.Run(src);
        }

        // public object arg { get; set; }
        // public 
        // public override async Task Execute(Decision decision)
        // {
        //     arg = null;
        //     await base.Execute(decision);
        // }

        public override Decision AIDecision() => AI.TryAction();

        // public virtual async Task Execute(){await Task.Yield();}


    }
}
