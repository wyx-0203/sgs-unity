using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Model
{
    public class PlayerEvents
    {
        public PlayerEvents()
        {
            StartPhase = new Dictionary<Phase, EventSet>();
            FinishPhase = new Dictionary<Phase, EventSet>();

            foreach (Phase phase in System.Enum.GetValues(typeof(Phase)))
            {
                StartPhase.Add(phase, new EventSet());
                FinishPhase.Add(phase, new EventSet());
            }

            WhenGetCard = new EventSet<GetCardFromPile>();
            AfterGetCard = new EventSet<GetCard>();
            LoseCard = new EventSet<LoseCard>();

            Recover = new EventSet<Recover>();
            WhenDamaged = new EventSet<Damaged>();
            AfterDamaged = new EventSet<Damaged>();
            AfterLoseHp = new EventSet<UpdateHp>();

            WhenUseCard = new EventSet<Card>();
            AfterUseCard = new EventSet<Card>();
        }

        // 阶段开始时事件
        public Dictionary<Phase, EventSet> StartPhase { get; private set; }
        // 阶段结束时事件
        public Dictionary<Phase, EventSet> FinishPhase { get; private set; }

        // 摸牌时事件
        public EventSet<GetCardFromPile> WhenGetCard { get; private set; }
        // 获得牌后事件
        public EventSet<GetCard> AfterGetCard { get; private set; }
        // 失去牌后事件
        public EventSet<LoseCard> LoseCard { get; private set; }

        // 回复体力后事件
        public EventSet<Recover> Recover { get; private set; }

        // 受到伤害时事件
        public EventSet<Damaged> WhenDamaged { get; private set; }
        // 受到伤害后事件
        public EventSet<Damaged> AfterDamaged { get; private set; }
        // 失去体力后事件
        public EventSet<UpdateHp> AfterLoseHp { get; private set; }

        // 使用牌时事件
        public EventSet<Card> WhenUseCard { get; private set; }
        // 使用牌后事件
        public EventSet<Card> AfterUseCard { get; private set; }

        public void Clear()
        {
            foreach (Phase phase in System.Enum.GetValues(typeof(Phase)))
            {
                StartPhase[phase].Clear();
                FinishPhase[phase].Clear();
            }

            WhenGetCard.Clear();
            AfterGetCard.Clear();
            LoseCard.Clear();

            Recover.Clear();
            WhenDamaged.Clear();
            AfterDamaged.Clear();
            AfterLoseHp.Clear();

            WhenUseCard.Clear();
            AfterUseCard.Clear();
        }
    }

    /// <summary>
    /// 一个操作或阶段所触发的操作请求集合
    /// </summary>
    public class EventSet
    {
        /// <summary>
        /// 用字典维护每名玩家的操作请求
        /// </summary>
        public Dictionary<Player, Func<Task>> events;

        /// <summary>
        /// 添加操作请求
        /// </summary>
        public void AddEvent(Player player, Func<Task> request)
        {
            if (events is null) events = new Dictionary<Player, Func<Task>>();

            if (!events.ContainsKey(player)) events.Add(player, request);
            // else events[player] += request;
        }

        /// <summary>
        /// 删除操作请求
        /// </summary>
        public void RemoveEvent(Player player)
        {
            if (events is null) return;
            events.Remove(player);
        }

        // 询问请求
        public async Task Execute()
        {
            // if (events is null) return;

            await Util.Loop(async x =>
            {
                if (events.ContainsKey(x)) await events[x]();
            }, () => events != null);
        }

        public void Clear()
        {
            events = null;
        }
    }

    /// <summary>
    /// 带参数的EventSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventSet<T>
    {

        /// <summary>
        /// 用字典维护每名玩家的操作请求
        /// </summary>
        private Dictionary<Player, Func<T, Task>> events;

        /// <summary>
        /// 添加操作请求
        /// </summary>
        public void AddEvent(Player player, Func<T, Task> request)
        {
            if (events is null) events = new Dictionary<Player, Func<T, Task>>();

            if (!events.ContainsKey(player)) events.Add(player, request);
        }

        /// <summary>
        /// 删除操作请求
        /// </summary>
        public void RemoveEvent(Player player)
        {
            if (events is null) return;
            if (events.ContainsKey(player)) events.Remove(player);
        }

        /// <summary>
        /// 询问请求
        /// </summary>
        public async Task Execute(T param)
        {
            // if (events is null) return;

            await Util.Loop(async x =>
            {
                if (events.ContainsKey(x)) await events[x](param);
            }, () => events != null);
        }

        public void Clear()
        {
            events = null;
        }
    }
}
