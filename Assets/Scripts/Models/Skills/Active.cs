using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 主动技
    /// </summary>
    public class Active : Skill
    {
        public Active(Player src) : base(src) { }
        public override int TimeLimit => 1;

        /// <summary>
        /// 发动技能
        /// </summary>
        /// <param name="dests">选中目标</param>
        /// <param name="cards">选中卡牌</param>
        /// <param name="other">附加信息</param>
        public virtual async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            await Task.Yield();
            Debug.Log(Src.posStr + "号位使用了" + Name);
            Time++;
            // Dests = dests;
            Execute();
        }

        public override bool IsValid => Timer.Instance.isPerformPhase && base.IsValid;

        public override void OnEnable()
        {
            TurnSystem.Instance.AfterPerform += Reset;
        }

        public override void OnDisable()
        {
            TurnSystem.Instance.AfterPerform -= Reset;
        }
    }
}