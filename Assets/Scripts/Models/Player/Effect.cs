using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public enum LifeType
    {
        UntilPlayPhaseEnd,
        UntilTurnEnd,
    }

    public interface Executable
    {
        public void Execute();
        public Action OnRemove { get; set; }
        public bool enabled { get; }
    }

    public class EffectCollection
    {
        public EffectCollection(Player src) => this.src = src;
        private Player src;

        /// <summary>
        /// 无次数限制
        /// </summary>
        public NoTimesLimit NoTimesLimit = new();

        /// <summary>
        /// 无距离限制
        /// </summary>
        public NoDistanceLimit NoDistanceLimit = new();

        /// <summary>
        /// 卡牌对玩家无效
        /// </summary>
        public InvalidForDest InvalidForDest = new();

        /// <summary>
        /// 不可响应
        /// </summary>
        public Unmissable Unmissable = new();

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        public DisableCard DisableCard = new();

        /// <summary>
        /// 禁用技能
        /// </summary>
        public DisableSkill DisableSkill = new();

        /// <summary>
        /// 伤害值偏移
        /// </summary>
        public OffsetDamageValue OffsetDamageValue = new();

        /// <summary>
        /// 额外目标
        /// </summary>
        public ExtraDestCount ExtraDestCount = new();

        public void NoAttactRangeLimit(LifeType lifeType)
        {
            src.AttackRange += 20;
            if (lifeType == LifeType.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlayOnce += () => src.AttackRange -= 20;
            else if (lifeType == LifeType.UntilTurnEnd) TurnSystem.Instance.AfterTurnOnce += () => src.AttackRange -= 20;
        }
    }

    public class Effect<T>
    {
        private Predicate<T> effects;
        private List<Predicate<T>> onceEffects = new();
        private Dictionary<Predicate<T>, Executable> skills = new();
        private Executable currentSkill;

        public void Add(Predicate<T> predicate, LifeType lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == LifeType.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlayOnce += () => effects -= predicate;
            else if (lifeType == LifeType.UntilTurnEnd) TurnSystem.Instance.AfterTurnOnce += () => effects -= predicate;
        }

        public void Add(Predicate<T> predicate, Executable skill)
        {
            effects += predicate;
            skill.OnRemove += () =>
            {
                effects -= predicate;
                skills.Remove(predicate);
            };
            skills.Add(predicate, skill);
        }

        public bool Invoke(T arg)
        {
            if (effects is null) return false;
            foreach (Predicate<T> i in effects.GetInvocationList())
            {
                if (skills.ContainsKey(i) && !skills[i].enabled) continue;
                if (!i(arg)) continue;
                if (onceEffects.Contains(i))
                {
                    effects -= i;
                    onceEffects.Remove(i);
                }
                if (skills.ContainsKey(i)) currentSkill = skills[i];
                return true;
            }
            return false;
        }

        public void TryExecute()
        {
            if (currentSkill is null) return;
            currentSkill.Execute();
            currentSkill = null;
        }
    }

    public class EffectInt<T>
    {
        private Func<T, int> effects;
        private List<Func<T, int>> onceEffects = new();
        private Dictionary<Func<T, int>, Executable> skills = new();
        private Executable currentSkill;

        public void Add(Func<T, int> predicate, LifeType lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == LifeType.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlayOnce += () => effects -= predicate;
            else if (lifeType == LifeType.UntilTurnEnd) TurnSystem.Instance.AfterTurnOnce += () => effects -= predicate;
        }

        public void Add(Func<T, int> predicate, Executable skill)
        {
            effects += predicate;
            skill.OnRemove += () =>
            {
                effects -= predicate;
                skills.Remove(predicate);
            };
            skills.Add(predicate, skill);
        }

        public int Invoke(T arg)
        {
            if (effects is null) return 0;
            int res = 0;
            foreach (Func<T, int> i in effects.GetInvocationList())
            {
                if (skills.ContainsKey(i) && !skills[i].enabled) continue;
                int t = i(arg);
                if (t == 0) continue;
                if (onceEffects.Contains(i))
                {
                    Util.Print("onceEffects.Contains(i)");
                    effects -= i;
                    onceEffects.Remove(i);
                }
                if (skills.ContainsKey(i)) currentSkill = skills[i];
                res += t;
            }
            return res;
        }

        public void TryExecute()
        {
            if (currentSkill is null) return;
            currentSkill.Execute();
            currentSkill = null;
        }
    }

    public class NoTimesLimit : Effect<Card> { }

    public class InvalidForDest : Effect<Card> { }

    public class NoDistanceLimit : Effect<Tuple<Card, Player>>
    {
        public void Add(Func<Card, Player, bool> predicate, LifeType lifeType, bool once = false)
        {
            Add(x => predicate(x.Item1, x.Item2), lifeType, once);
        }

        public void Add(Func<Card, Player, bool> predicate, Executable skill)
        {
            Add(x => predicate(x.Item1, x.Item2), skill);
        }

        public bool Invoke(Card card, Player player) => Invoke(new Tuple<Card, Player>(card, player));
    }

    public class Unmissable : Effect<Tuple<Card, Player>>
    {
        public void Add(Func<Card, Player, bool> predicate, LifeType lifeType, bool once = false)
        {
            Add(x => predicate(x.Item1, x.Item2), lifeType, once);
        }

        public void Add(Func<Card, Player, bool> predicate, Executable skill)
        {
            Add(x => predicate(x.Item1, x.Item2), skill);
        }

        public bool Invoke(Card card, Player player) => Invoke(new Tuple<Card, Player>(card, player));
    }

    public class DisableCard : Effect<Card> { }

    public class DisableSkill : Effect<Skill> { }

    public class OffsetDamageValue : EffectInt<Damaged> { }

    public class ExtraDestCount : EffectInt<Card> { }
}
