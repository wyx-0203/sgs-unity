using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace Model
{
    /// <summary>
    /// 技能基类
    /// </summary>
    public class Skill
    {
        // 所属玩家
        public Player Src { get; private set; }
        // 技能名称
        public string Name { get; set; }
        // 锁定技
        public virtual bool Passive => false;
        // 限定技
        // public virtual bool Ultimate => false;
        // public bool UltimateIsDone { get; protected set; }
        // 限定次数
        public virtual int TimeLimit => int.MaxValue;
        // 已使用次数
        public int Time { get; set; }

        public Skill(Player src)
        {
            Src = src;
            // Name = name;
            // Passive = passive;
            // TimeLimit = timeLimit;

            SetActive(true);
        }

        /// <summary>
        /// 最大可选卡牌数
        /// </summary>
        public virtual int MaxCard => 0;

        /// <summary>
        /// 最小可选卡牌数
        /// </summary>
        public virtual int MinCard => 0;

        /// <summary>
        /// 判断卡牌是否可选
        /// </summary>
        public virtual bool IsValidCard(Card card) => true;

        /// <summary>
        /// 最大目标数
        /// </summary>
        public virtual int MaxDest => 0;

        /// <summary>
        /// 最小目标数
        /// </summary>
        public virtual int MinDest => 0;

        /// <summary>
        /// 判断目标是否可选
        /// </summary>
        /// <param name="dest">目标</param>
        public virtual bool IsValidDest(Player dest) => true;

        /// <summary>
        /// 是否有效
        /// </summary>
        public int Enabled { get; set; } = 0;

        public void SetActive(bool valid)
        {
            if (valid)
            {
                if (Enabled > 0) return;
                Enabled++;
                if (Enabled > 0) OnEnable();
            }
            else
            {
                if (Enabled <= 0) return;
                Enabled--;
                if (Enabled <= 0) OnDisable();
            }

        }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        /// <summary>
        /// 技能是否满足条件
        /// </summary>
        public virtual bool IsValid => Time < TimeLimit
            && Enabled > 0
            && (this is not Ultimate || !(this as Ultimate).IsDone);

        public virtual void Execute()
        {
            Time++;
            // if (this is Ultimate) (this as Ultimate).IsDone = true;
            Dests = Timer.Instance.dests;
            useSkillView(this);
        }

        protected virtual void Reset()
        {
            Time = 0;
        }

        public List<Player> Dests { get; private set; }

        protected Player firstDest => Operation.Instance.Dests.Count == 0 ? null : Operation.Instance.Dests[0];

        protected bool isAI => Room.Instance.IsSingle && Src.isAI;

        private static UnityAction<Skill> useSkillView;
        public static event UnityAction<Skill> UseSkillView
        {
            add => useSkillView += value;
            remove => useSkillView -= value;
        }

        public static Dictionary<string, Type> SkillMap { get; set; } = new Dictionary<string, Type>
        {
            { "仁德", typeof(仁德) },
            { "武圣", typeof(武圣) },
            { "义绝", typeof(义绝) },
            { "咆哮", typeof(咆哮) },
            { "制衡", typeof(制衡) },
            { "苦肉", typeof(苦肉) },
            { "诈降", typeof(诈降) },
            { "奸雄", typeof(奸雄) },
            { "刚烈", typeof(刚烈) },
            { "清俭", typeof(清俭) },
            { "突袭", typeof(突袭) },
            { "无双", typeof(无双) },
            { "利驭", typeof(利驭) },
            { "离间", typeof(离间) },
            { "闭月", typeof(闭月) },
            { "驱虎", typeof(驱虎) },
            { "节命", typeof(节命) },
            { "好施", typeof(好施) },
            { "缔盟", typeof(缔盟) },
            { "恩怨", typeof(恩怨) },
            { "眩惑", typeof(眩惑) },
            { "散谣", typeof(散谣) },
            { "制蛮", typeof(制蛮) },
            { "明策", typeof(明策) },
            { "智迟", typeof(智迟) },
            { "烈弓", typeof(烈弓) },
            { "乱击", typeof(乱击) },
            { "父魂", typeof(父魂) },
            { "享乐", typeof(享乐) },
            { "放权", typeof(放权) },
            // { "当先", typeof(当先) },
            { "弓骑", typeof(弓骑) },
            { "解烦", typeof(解烦) },
            { "精策", typeof(精策) },
            { "龙吟", typeof(龙吟) },
            { "竭忠", typeof(竭忠) },
            { "千驹", typeof(千驹) },
            { "倾袭", typeof(倾袭) },
        };
    }

    public interface Ultimate
    {
        public bool IsDone { get; set; }
    }
}