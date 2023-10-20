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
            await player.events.AfterGetCard.Execute(this);
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
            await player.events.WhenGetCard.Execute(this);
            if (Count == 0) return;
            Util.Print(player + "摸了" + Count + "张牌");
            // 摸牌
            for (int i = 0; i < Count; i++) Cards.Add(await CardPile.Instance.Pop());
            await base.Execute();
        }

        public bool InGetCardPhase { get; set; } = false;
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
            await player.events.LoseCard.Execute(this);
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

            // losecard
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
            Value = value;
        }
        public int Value { get; set; }

        public async Task Execute()
        {
            // 更新体力
            player.Hp += Value;
            actionView(this);

            // 濒死
            if (player.Hp < 1 && Value < 0) await NearDeath();

            // 失去体力
            if (Value < 0 && this is not Damaged)
            {
                Util.Print(player + "失去了了" + (-Value) + "点体力");
                await player.events.AfterLoseHp.Execute(this);
            }
        }

        private async Task NearDeath()
        {
            Util.Print(player + "进入濒死状态");
            var currentPlayer = TurnSystem.Instance.CurrentPlayer;
            bool t = true;
            for (var i = currentPlayer; i != currentPlayer || t; i = i.next)
            {
                t = false;
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

            while (player.skills.Count > 0) player.skills[0].Remove();
            player.events.Clear();

            if (player.IsLocked) await new SetLock(player).Execute();

            player.IsAlive = false;
            player.next.last = player.last;
            player.last.next = player.next;
            SgsMain.Instance.AlivePlayers.Remove(player);

            // 弃置所有牌
            // var cards = player.HandCards.Union(player.Equipments.Values).ToList();
            await new Discard(player, player.cards.ToList()).Execute();

            foreach (var i in new List<DelayScheme>(player.JudgeCards))
            {
                i.RemoveToJudgeArea();
                CardPile.Instance.AddToDiscard(i);
            }

            player.teammates.Remove(player);

            await Mode.Instance.WhenPlayerDie(player, DamageSrc);

            // if (player.teammates.Count == 0) throw new GameOverException(player.team);
            // else if (SgsMain.Instance.mode is Mode.统帅双军) await new GetCardFromPile(player.teammates[0], 1).Execute();

            if (player == TurnSystem.Instance.CurrentPlayer) throw new CurrentPlayerDie();
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
            int t = player.Hp + Value - player.HpLimit;
            if (t > 0)
            {
                Value -= t;
                if (Value == 0) return;
            }

            Util.Print(player + "回复了" + Value + "点体力");

            // 回复体力
            await base.Execute();

            // 执行事件
            await player.events.Recover.Execute(this);
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
        public Damaged(Player player, Player src, Card srcCard = null, int value = 1, DamageType type = DamageType.Normal)
            : base(player, -value)
        {
            Src = src;
            SrcCard = srcCard;
            damageType = type;
        }

        public Player Src { get; private set; }
        public Card SrcCard { get; private set; }
        public DamageType damageType { get; private set; }
        public bool IsConDucted { get; set; } = false;
        private bool conduct = false;

        public new async Task Execute()
        {
            // 受到伤害时
            try { await player.events.WhenDamaged.Execute(this); }
            catch (PreventDamage) { return; }

            // 藤甲 白银狮子
            if (player.armor != null && !(SrcCard is 杀 sha && sha.IgnoreArmor)) player.armor.WhenDamaged(this);

            Util.Print(player + "受到了" + (-Value) + "点伤害");

            // 解锁
            if (damageType != DamageType.Normal && player.IsLocked)
            {
                await new SetLock(player, true).Execute();
                if (!IsConDucted) conduct = true;
            }

            try
            {
                // 受到伤害
                await base.Execute();
                // 受到伤害后
                await player.events.AfterDamaged.Execute(this);
            }
            catch (PlayerDie) { }
            catch (CurrentPlayerDie) { throw; }

            // 铁锁传导
            finally { if (conduct) await Conduct(); }
        }

        /// <summary>
        /// 铁索连环传导
        /// </summary>
        private async Task Conduct()
        {
            Func<Player, Task> func = async x =>
            {
                if (!x.IsLocked) return;

                var damaged = new Damaged(x, Src, SrcCard, -Value, damageType);
                damaged.IsConDucted = true;
                await damaged.Execute();
            };

            await Util.Loop(func);
        }
    }

    /// <summary>
    /// 获得其他角色的牌
    /// </summary>
    public class GetCardFromElse : GetCard
    {
        public GetCardFromElse(Player player, Player dest, List<Card> cards) : base(player, cards)
        {
            Dest = dest;
            Cards = cards;
        }
        public Player Dest { get; private set; }

        public new async Task Execute()
        {
            // 获得牌
            Util.Print(player + "获得了" + Dest + "的" + Cards.Count + "张牌");
            player.HandCards.AddRange(Cards);
            foreach (var i in Cards) i.Src = player;

            actionView(this);

            // 目标失去牌
            await new LoseCard(Dest, Cards).Execute();

            // 执行获得牌后事件
            await player.events.AfterGetCard.Execute(this);
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
            player.IsLocked = !player.IsLocked;
            actionView(this);
            await Task.Yield();
        }
    }

    public class Compete : PlayerAction<Compete>
    {
        public Compete(Player player, Player dest) : base(player)
        {
            this.dest = dest;
        }

        private Player dest;
        private Card card0;
        private Card card1;
        private bool Result;

        public async Task<bool> Execute()
        {
            Timer.Instance.hint = "请选择一张手牌拼点";
            if (player.team == dest.team)
            {
                card0 = (await TimerAction.SelectHandCard(player, 1))[0];
                card1 = (await TimerAction.SelectHandCard(dest, 1))[0];
            }
            else
            {
                var result = await CompeteTimer.Instance.Run(player, dest);
                card0 = result[player];
                card1 = result[dest];
            }

            CardPile.Instance.AddToDiscard(card0);
            CardPile.Instance.AddToDiscard(card1);
            await new LoseCard(player, new List<Card> { card0 }).Execute();
            await new LoseCard(dest, new List<Card> { card1 }).Execute();

            return card0.weight > card1.weight;
        }
    }

    public class UpdateSkill : PlayerAction<UpdateSkill>
    {
        public UpdateSkill(Player player, List<string> skills) : base(player)
        {
            Skills = skills;
        }
        public List<string> Skills { get; protected set; }

        public void Add()
        {
            foreach (var i in Skills)
            {
                var skill = Activator.CreateInstance(Skill.SkillMap[i]) as Skill;
                skill.Init(i, player);
            }
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

    // 翻面
}