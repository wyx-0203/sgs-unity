using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore
{
    public enum Duration
    {
        UntilPlayPhaseEnd,
        UntilTurnEnd,
    }

    public interface IExecutable
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
        public NoTimesLimit NoTimesLimit { get; } = new();

        /// <summary>
        /// 无距离限制
        /// </summary>
        public NoDistanceLimit NoDistanceLimit { get; } = new();

        /// <summary>
        /// 卡牌对玩家无效
        /// </summary>
        public InvalidForDest InvalidForDest { get; } = new();

        /// <summary>
        /// 不可响应
        /// </summary>
        public Unmissable Unmissable { get; } = new();

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        public DisableCard DisableCard { get; } = new();

        /// <summary>
        /// 禁用技能
        /// </summary>
        public DisableSkill DisableSkill { get; } = new();

        /// <summary>
        /// 需要两张牌响应 (杀、决斗)
        /// </summary>
        // public DestNeedsDoubleResponse DestNeedsDoubleResponse { get; } = new();

        /// <summary>
        /// 成为决斗目标时，使用者需打出两张杀
        /// </summary>
        // public SrcNeedsDoubleSha SrcNeedsDoubleSha { get; } = new();

        /// <summary>
        /// 伤害值偏移
        /// </summary>
        public OffsetDamageValue OffsetDamageValue { get; } = new();

        /// <summary>
        /// 额外目标
        /// </summary>
        public ExtraDestCount ExtraDestCount { get; } = new();

        public void NoAttactRangeLimit(Duration lifeType)
        {
            src.attackRange += 20;
            if (lifeType == Duration.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlay += () => src.attackRange -= 20;
            else if (lifeType == Duration.UntilTurnEnd) TurnSystem.Instance.AfterTurn += () => src.attackRange -= 20;
        }
    }

    public class Effect<T>
    {
        private Predicate<T> effects;
        private List<Predicate<T>> onceEffects = new();
        private Dictionary<Predicate<T>, IExecutable> skills = new();
        private IExecutable currentSkill;

        public void Add(Predicate<T> predicate, Duration lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == Duration.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlay += () => effects -= predicate;
            else if (lifeType == Duration.UntilTurnEnd) TurnSystem.Instance.AfterTurn += () => effects -= predicate;
        }

        public void Add(Predicate<T> predicate, IExecutable skill)
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

    public class Effect<T1, T2> : Effect<Tuple<T1, T2>>
    {
        public void Add(Func<T1, T2, bool> predicate, Duration lifeType, bool once = false)
        {
            Add(x => predicate(x.Item1, x.Item2), lifeType, once);
        }

        public void Add(Func<T1, T2, bool> predicate, IExecutable skill)
        {
            Add(x => predicate(x.Item1, x.Item2), skill);
        }

        public bool Invoke(T1 t1, T2 t2) => Invoke(new Tuple<T1, T2>(t1, t2));
    }

    public class EffectInt<T>
    {
        private Func<T, int> effects;
        private List<Func<T, int>> onceEffects = new();
        private Dictionary<Func<T, int>, IExecutable> skills = new();
        private IExecutable currentSkill;

        public void Add(Func<T, int> predicate, Duration lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == Duration.UntilPlayPhaseEnd) TurnSystem.Instance.AfterPlay += () => effects -= predicate;
            else if (lifeType == Duration.UntilTurnEnd) TurnSystem.Instance.AfterTurn += () => effects -= predicate;
        }

        public void Add(Func<T, int> predicate, IExecutable skill)
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

    public class NoDistanceLimit : Effect<Card, Player> { }
    // {
    //     public void Add(Func<Card, Player, bool> predicate, Duration lifeType, bool once = false)
    //     {
    //         Add(x => predicate(x.Item1, x.Item2), lifeType, once);
    //     }

    //     public void Add(Func<Card, Player, bool> predicate, IExecutable skill)
    //     {
    //         Add(x => predicate(x.Item1, x.Item2), skill);
    //     }

    //     public bool Invoke(Card card, Player player) => Invoke(new Tuple<Card, Player>(card, player));
    // }

    public class Unmissable : Effect<Card, Player> { }
    // {
    //     public void Add(Func<Card, Player, bool> predicate, Duration lifeType, bool once = false)
    //     {
    //         Add(x => predicate(x.Item1, x.Item2), lifeType, once);
    //     }

    //     public void Add(Func<Card, Player, bool> predicate, IExecutable skill)
    //     {
    //         Add(x => predicate(x.Item1, x.Item2), skill);
    //     }

    //     public bool Invoke(Card card, Player player) => Invoke(new Tuple<Card, Player>(card, player));
    // }

    public class DisableCard : Effect<Card> { }

    public class DisableSkill : Effect<Skill> { }

    // public class DestNeedsDoubleResponse : Effect<Card, Player> { }

    // public class SrcNeedsDoubleSha : Effect<决斗> { }

    public class OffsetDamageValue : EffectInt<Damage> { }

    public class ExtraDestCount : EffectInt<Card> { }
}
