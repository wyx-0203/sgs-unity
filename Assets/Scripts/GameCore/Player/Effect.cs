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
        public EffectCollection(Player src)
        {
            this.src = src;
            NoTimesLimit = new NoTimesLimit(src);
            NoDistanceLimit = new NoDistanceLimit(src);
            InvalidForDest = new InvalidForDest(src);
            Unmissable = new Unmissable(src);
            DisableCard = new DisableCard(src);
            DisableSkill = new DisableSkill(src);
            OffsetDamageValue = new OffsetDamageValue(src);
            ExtraDestCount = new ExtraDestCount(src);
        }
        private Player src;
        // private Game game=>src.game;

        /// <summary>
        /// 无次数限制
        /// </summary>
        public NoTimesLimit NoTimesLimit { get; }

        /// <summary>
        /// 无距离限制
        /// </summary>
        public NoDistanceLimit NoDistanceLimit { get; }

        /// <summary>
        /// 卡牌对玩家无效
        /// </summary>
        public InvalidForDest InvalidForDest { get; }
        /// <summary>
        /// 不可响应
        /// </summary>
        public Unmissable Unmissable { get; }

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        public DisableCard DisableCard { get; }

        /// <summary>
        /// 禁用技能
        /// </summary>
        public DisableSkill DisableSkill { get; }

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
        public OffsetDamageValue OffsetDamageValue { get; }

        /// <summary>
        /// 额外目标
        /// </summary>
        public ExtraDestCount ExtraDestCount { get; }
        public void NoAttactRangeLimit(Duration lifeType)
        {
            src.attackRange += 20;
            if (lifeType == Duration.UntilPlayPhaseEnd) src.game.turnSystem.AfterPlay += () => src.attackRange -= 20;
            else if (lifeType == Duration.UntilTurnEnd) src.game.turnSystem.AfterTurn += () => src.attackRange -= 20;
        }
    }

    public class Effect<T>
    {
        public Effect(Player player)
        {
            this.player = player;
        }
        private Player player;
        protected Game game => player.game;

        private Predicate<T> effects;
        private List<Predicate<T>> onceEffects = new();
        private Dictionary<Predicate<T>, IExecutable> skills = new();
        private IExecutable currentSkill;

        public void Add(Predicate<T> predicate, Duration lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == Duration.UntilPlayPhaseEnd) game.turnSystem.AfterPlay += () => effects -= predicate;
            else if (lifeType == Duration.UntilTurnEnd) game.turnSystem.AfterTurn += () => effects -= predicate;
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
        public Effect(Player player) : base(player) { }

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
        public EffectInt(Player player)
        {
            game = player.game;
        }
        protected Game game;

        private Func<T, int> effects;
        private List<Func<T, int>> onceEffects = new();
        private Dictionary<Func<T, int>, IExecutable> skills = new();
        private IExecutable currentSkill;

        public void Add(Func<T, int> predicate, Duration lifeType, bool once = false)
        {
            effects += predicate;
            if (once) onceEffects.Add(predicate);
            if (lifeType == Duration.UntilPlayPhaseEnd) game.turnSystem.AfterPlay += () => effects -= predicate;
            else if (lifeType == Duration.UntilTurnEnd) game.turnSystem.AfterTurn += () => effects -= predicate;
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
                    // Util.Print("onceEffects.Contains(i)");
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

    public class NoTimesLimit : Effect<Card>
    {
        public NoTimesLimit(Player player) : base(player)
        {
        }
    }

    public class InvalidForDest : Effect<Card>
    {
        public InvalidForDest(Player player) : base(player)
        {
        }
    }

    public class NoDistanceLimit : Effect<Card, Player>
    {
        public NoDistanceLimit(Player player) : base(player)
        {
        }
    }
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

    public class Unmissable : Effect<Card, Player>
    {
        public Unmissable(Player player) : base(player)
        {
        }
    }
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

    public class DisableCard : Effect<Card>
    {
        public DisableCard(Player player) : base(player)
        {
        }
    }

    public class DisableSkill : Effect<Skill>
    {
        public DisableSkill(Player player) : base(player)
        {
        }
    }

    // public class DestNeedsDoubleResponse : Effect<Card, Player> { }

    // public class SrcNeedsDoubleSha : Effect<决斗> { }

    public class OffsetDamageValue : EffectInt<Damage>
    {
        public OffsetDamageValue(Player player) : base(player)
        {
        }
    }

    public class ExtraDestCount : EffectInt<Card>
    {
        public ExtraDestCount(Player player) : base(player)
        {
        }
    }
}
