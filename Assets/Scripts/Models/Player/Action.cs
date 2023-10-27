using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Model
{
    public class PlayerAction<T>
    {
        public Player player { get; private set; }
        public PlayerAction(Player player)
        {
            this.player = player;
        }

        protected static void actionView(T arg)
        {
            if (!MCTS.Instance.isRunning) ActionView?.Invoke(arg);
        }
        public static UnityAction<T> ActionView { get; set; }
    }

    /// <summary>
    /// 获得牌
    /// </summary>
    public class GetCard : PlayerAction<GetCard>
    {
        /// <summary>
        /// 获得牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="cards">卡牌数组</param>
        public GetCard(Player player, List<Card> cards) : base(player) => Cards = cards;
        public List<Card> Cards { get; protected set; }

        public async Task Execute()
        {
            // 获得牌
            player.HandCards.AddRange(Cards);
            foreach (var i in Cards) i.Src = player;

            actionView(this);

            // 执行获得牌后事件
            await EventSystem.Instance.Invoke(x => x.OnEveryGetCard, this);
        }
    }

    /// <summary>
    /// 摸牌
    /// </summary>
    public class GetCardFromPile : GetCard
    {
        /// <summary>
        /// 摸牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="count">摸牌数</param>
        public GetCardFromPile(Player player, int count) : base(player, new List<Card>()) => Count = count;
        public int Count { get; set; }

        public new async Task Execute()
        {
            await EventSystem.Instance.Invoke(x => x.BeforeEveryGetCardFromPile, this);

            if (Count == 0) return;
            Util.Print(player + "摸了" + Count + "张牌");
            // 摸牌
            for (int i = 0; i < Count; i++) Cards.Add(await CardPile.Instance.Pop());
            await base.Execute();
        }

        public bool inGetPhase { get; set; } = false;
    }

    public class GetDisCard : GetCard
    {
        public GetDisCard(Player player, List<Card> cards) : base(player, cards) { }

        public new async Task Execute()
        {
            if (Cards.Count == 0) return;

            Util.Print(player + "从弃牌堆获得了" + string.Join("、", Cards));
            foreach (var i in Cards) CardPile.Instance.DiscardPile.Remove(i);
            await base.Execute();
        }
    }

    public class GetJudgeCard : GetCard
    {
        public GetJudgeCard(Player player, Card card) : base(player, new List<Card> { card }) { }

        public new async Task Execute()
        {
            var card = Cards[0];

            (card as DelayScheme).RemoveToJudgeArea();
            Util.Print(player + "获得了" + string.Join("、", Cards));
            await base.Execute();
        }
    }

    /// <summary>
    /// 失去牌
    /// </summary>
    public class LoseCard : PlayerAction<LoseCard>
    {
        /// <summary>
        /// 失去牌
        /// </summary>
        public LoseCard(Player player, List<Card> cards) : base(player) => Cards = cards;
        public List<Card> Cards { get; private set; }

        public async Task Execute()
        {
            foreach (var card in Cards)
            {
                if (player.HandCards.Contains(card)) player.HandCards.Remove(card);
                else if (card is Equipment equipage) await equipage.Remove();
            }

            actionView(this);

            // 执行失去牌后事件
            await EventSystem.Instance.Invoke(x => x.OnEveryLoseCard, this);
        }
    }

    /// <summary>
    /// 弃牌
    /// </summary>
    public class Discard : LoseCard
    {
        /// <summary>
        /// 弃牌
        /// </summary>
        public Discard(Player player, List<Card> cards) : base(player, cards) { }

        public new async Task Execute()
        {
            if (Cards is null || Cards.Count == 0) return;
            Util.Print(player + "弃置了" + string.Join("、", Cards));

            CardPile.Instance.AddToDiscard(Cards);

            await base.Execute();
        }
    }

    public class UpdateHp : PlayerAction<UpdateHp>
    {
        /// <summary>
        /// 改变体力
        /// </summary>
        public UpdateHp(Player player, int value) : base(player)
        {
            this.value = value;
        }
        public int value { get; set; }

        public async Task Execute()
        {
            // 更新体力
            player.Hp += value;
            actionView(this);

            // 失去体力
            if (value < 0 && this is not Damaged)
            {
                Util.Print(player + "失去了" + (-value) + "点体力");
            }

            // 濒死
            if (player.Hp < 1 && value < 0) await NearDeath();

            // 改变体力后事件
            await EventSystem.Instance.Invoke(x => x.OnEveryUpdateHp, this);
        }

        private async Task NearDeath()
        {
            Util.Print(player + "进入濒死状态");

            foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
            {
                while (await 桃.Call(i, player))
                {
                    if (player.Hp >= 1) return;
                }
            }

            await new Die(player, this is Damaged damaged && damaged.Src != player ? damaged.Src : null).Execute();
        }
    }

    /// <summary>
    /// 阵亡
    /// </summary>
    public class Die : PlayerAction<Die>
    {
        public Die(Player player, Player damageSrc) : base(player)
        {
            DamageSrc = damageSrc;
        }

        public Player DamageSrc { get; private set; }

        public async Task Execute()
        {
            actionView(this);

            // 清除技能
            while (player.skills.Count > 0) player.skills[0].Remove();

            // 解锁
            if (player.locked) await new SetLock(player).Execute();

            // 修改位置关系
            player.alive = false;
            player.next.last = player.last;
            player.last.next = player.next;
            player.teammates.Remove(player);
            SgsMain.Instance.AlivePlayers.Remove(player);

            // 弃置所有牌
            await new Discard(player, player.cards.ToList()).Execute();
            foreach (var i in new List<DelayScheme>(player.JudgeCards))
            {
                i.RemoveToJudgeArea();
                CardPile.Instance.AddToDiscard(i);
            }

            // 执行当前模式的阵亡时事件
            await Mode.Instance.OnPlayerDie(player, DamageSrc);
            throw new PlayerDie();
        }
    }

    public class Recover : UpdateHp
    {
        /// <summary>
        /// 回复体力
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="value">回复量</param>
        public Recover(Player player, int value = 1) : base(player, value) { }

        public new async Task Execute()
        {
            // 判断体力是否超过上限
            int t = player.Hp + value - player.HpLimit;
            if (t > 0)
            {
                value -= t;
                if (value == 0) return;
            }

            Util.Print(player + "回复了" + value + "点体力");

            // 回复体力
            await base.Execute();

            // 执行事件
            await EventSystem.Instance.Invoke(x => x.OnEveryRecover, this);
        }
    }

    public class Damaged : UpdateHp
    {
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="src">伤害来源</param>
        /// <param name="value">伤害量</param>
        public Damaged(Player player, Player src, Card srcCard = null, int value = 1, Type type = Type.Normal)
            : base(player, -value)
        {
            Src = src;
            SrcCard = srcCard;
            this.type = type;
        }

        public enum Type
        {
            Normal,
            Fire,
            Thunder
        }

        public Player Src { get; private set; }
        public Card SrcCard { get; private set; }
        public Type type { get; private set; }
        public bool isConDucted { get; set; } = false;
        private bool conduct = false;
        public new int value { get => -base.value; set => base.value = -value; }

        public new async Task Execute()
        {
            // 受到伤害时
            try { await EventSystem.Instance.Invoke(x => x.BeforeEveryDamaged, this); }
            catch (PreventDamage) { return; }

            // 藤甲 义绝
            value += player.effects.OffsetDamageValue.Invoke(this);
            player.effects.OffsetDamageValue.TryExecute();
            // value += OffsetDamageValue.Instance.Invoke(this);
            // 白银狮子
            if (value > 1 && player.armor is 白银狮子 bysz)
            {
                value = 1;
                bysz.Execute();
            }
            Util.Print(player + "受到了" + value + "点伤害");

            // 解锁
            if (type != Type.Normal && player.locked)
            {
                await new SetLock(player, true).Execute();
                if (!isConDucted) conduct = true;
            }

            try
            {
                // 受到伤害
                await base.Execute();
                // 受到伤害后
                // await EventSystem.Instance.Invoke(x => x.OnEveryDamaged, this);
            }
            catch (PlayerDie) { }
            // catch (CurrentPlayerDie) { throw; }

            // 铁锁传导
            if (conduct) await Conduct();
            // finally { if (conduct) await Conduct(); }
        }

        /// <summary>
        /// 铁索连环传导
        /// </summary>
        private async Task Conduct()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers.Where(x => x.locked).OrderBy(x => x.orderKey))
            {
                var damaged = new Damaged(i, Src, SrcCard, value, type);
                damaged.isConDucted = true;
                await damaged.Execute();
            }
        }
    }

    /// <summary>
    /// 获得其他角色的牌
    /// </summary>
    public class GetCardFromElse : GetCard
    {
        public GetCardFromElse(Player player, Player dest, List<Card> cards) : base(player, cards)
        {
            this.dest = dest;
            Cards = cards;
        }
        public Player dest { get; private set; }

        public new async Task Execute()
        {
            // 获得牌
            Util.Print(player + "获得了" + dest + "的" + Cards.Count + "张牌");
            player.HandCards.AddRange(Cards);
            foreach (var i in Cards) i.Src = player;

            actionView(this);

            // 目标失去牌
            await new LoseCard(dest, Cards).Execute();

            // 执行获得牌后事件
            await EventSystem.Instance.Invoke(x => x.OnEveryGetCard, this);
        }
    }

    /// <summary>
    /// 判定
    /// </summary>
    public class Judge
    {
        public async Task<Card> Execute()
        {
            var JudgeCard = await CardPile.Instance.Pop();
            CardPile.Instance.AddToDiscard(JudgeCard);
            Util.Print("判定结果为" + JudgeCard);

            return JudgeCard;
        }
    }

    /// <summary>
    /// 展示手牌
    /// </summary>
    public class ShowCard : PlayerAction<ShowCard>
    {
        public ShowCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; protected set; }

        public async Task Execute()
        {
            await Task.Yield();
            Util.Print(player + "展示了" + string.Join("、", Cards));
            actionView(this);
        }
    }

    /// <summary>
    /// 横置 (重置)
    /// </summary>
    public class SetLock : PlayerAction<SetLock>
    {
        public SetLock(Player player, bool byDamage = false) : base(player)
        {
            ByDamage = byDamage;
        }

        public bool ByDamage { get; private set; }

        public async Task Execute()
        {
            player.locked = !player.locked;
            actionView(this);
            await Task.Yield();
        }
    }

    /// <summary>
    /// 获得或失去技能
    /// </summary>
    public class UpdateSkill : PlayerAction<UpdateSkill>
    {
        public UpdateSkill(Player player, List<string> skills) : base(player)
        {
            Skills = skills;
        }
        public List<string> Skills { get; protected set; }

        public void Add()
        {
            foreach (var i in Skills) Skill.New(i, player);
            actionView(this);
        }

        public void Remove()
        {
            foreach (var i in Skills)
            {
                var skill = player.FindSkill(i);
                if (skill != null) skill.Remove();
            }
            actionView(this);
        }
    }

    public class ExChange : PlayerAction<ExChange>
    {
        public ExChange(Player player, Player dest) : base(player)
        {
            Dest = dest;
        }

        public Player Dest { get; private set; }

        public async Task Execute()
        {
            actionView(this);
            var cards0 = new List<Card>(player.HandCards);
            var cards1 = new List<Card>(Dest.HandCards);
            await new LoseCard(player, cards0).Execute();
            await new LoseCard(Dest, cards1).Execute();
            await new GetCard(player, cards1).Execute();
            await new GetCard(Dest, cards0).Execute();
        }
    }

    // 翻面
    public class TurnOver : PlayerAction<TurnOver>
    {
        public TurnOver(Player player) : base(player) { }

        public async Task Execute()
        {
            player.turnOver = !player.turnOver;
            actionView(this);
            await EventSystem.Instance.Invoke(x => x.OnEveryTurnOver, this);
        }
    }
}