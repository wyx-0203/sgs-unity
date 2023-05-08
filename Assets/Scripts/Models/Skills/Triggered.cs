using System.Threading.Tasks;

namespace Model
{
    public class Triggered : Skill
    {
        public Triggered(Player src) : base(src) { }

        /// <summary>
        /// 询问是否发动技能
        /// </summary>
        protected async Task<bool> ShowTimer()
        {
            Timer.Instance.GivenSkill = Name;
            Timer.Instance.Hint = "是否发动" + Name + "？";
            if (isAI)
            {
                Timer.Instance.maxCard = MaxCard;
                Timer.Instance.minCard = MinCard;
                Timer.Instance.MaxDest = () => MaxDest;
                Timer.Instance.MinDest = () => MinDest;
                Timer.Instance.IsValidCard = IsValidCard;
                Timer.Instance.IsValidDest = IsValidDest;
            }
            return await Timer.Instance.Run(Src) || isAI && AIResult();
        }

        protected virtual bool AIResult() => true;
    }
}
