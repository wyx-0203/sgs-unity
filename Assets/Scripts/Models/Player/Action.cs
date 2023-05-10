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

        protected static UnityAction<T> actionView;
        public static event UnityAction<T> ActionView
        {
            add => actionView += value;
            remove => actionView -= value;
        }
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
        public GetCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; protected set; }

        public async Task Execute()
        {
            // 获得牌
            foreach (var card in Cards)
            {
                player.HandCards.Add(card);
                card.Src = player;
            }
            actionView?.Invoke(this);

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
        public GetCardFromPile(Player player, int count) : base(player, new List<Card>())
        {
            Count = count;
        }
        public int Count { get; set; }

        public new async Task Execute()
        {
            await player.events.WhenGetCard.Execute(this);
            if (Count == 0) return;
            Debug.Log(player.posStr + "号位摸了" + Count.ToString() + "张牌");
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
            var list = new List<Card>(Cards);
            Cards.Clear();
            foreach (var i in list) Cards.AddRange(i.InDiscardPile());
            foreach (var i in Cards) CardPile.Instance.DiscardPile.Remove(i);
            if (Cards.Count > 0) await base.Execute();
        }
    }

    public class GetJudgeCard : GetCard
    {
        public GetJudgeCard(Player player, Card card) : base(player, new List<Card> { card }) { }

        public new async Task Execute()
        {
            var card = Cards[0];

            (card as DelayScheme).RemoveToJudgeArea();
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
        public LoseCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; private set; }

        public async Task Execute()
        {
            foreach (var card in Cards)
            {
                if (player.HandCards.Contains(card)) player.HandCards.Remove(card);
                else if (card is Equipage) await (card as Equipage).RemoveEquipage();
            }

            actionView?.Invoke(this);

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
            string str = "";
            foreach (var card in Cards) str += "【" + card.Name + card.Suit + card.Weight.ToString() + "】";
            Debug.Log(player.posStr + "号位弃置了" + str);

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
            actionView?.Invoke(this);

            // 濒死
            if (player.Hp < 1 && Value < 0) await NearDeath();

            // 失去体力
            if (Value < 0 && this is not Damaged) await player.events.AfterLoseHp.Execute(this);
        }

        private async Task NearDeath()
        {
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

            await new Die(player, this is Damaged ? (this as Damaged).Src : null).Execute();
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
            actionView?.Invoke(this);

            foreach (var i in player.skills) i.SetActive(false);
            player.skills.Clear();
            player.events.Clear();

            if (player.IsLocked) await new SetLock(player).Execute();

            if (Room.Instance.IsSingle && player.isSelf) AI.Instance.DestList.Remove(player);

            player.IsAlive = false;
            player.next.last = player.last;
            player.last.next = player.next;
            SgsMain.Instance.AlivePlayers.Remove(player);

            // 弃置所有牌
            var cards = new List<Card>(player.HandCards);
            cards.AddRange(player.Equipages.Values.Where(x => x != null));
            await new Discard(player, cards).Execute();

            foreach (var i in player.JudgeArea)
            {
                i.RemoveToJudgeArea();
                CardPile.Instance.AddToDiscard(i);
            }

            if (!player.teammate.IsAlive) GameOver.Instance.Init(player.team);
            else await new GetCardFromPile(player.teammate, 1).Execute();
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

            Debug.Log(player.posStr + "回复了" + Value.ToString() + "点体力");

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
            await player.events.WhenDamaged.Execute(this);

            if (Value == 0) return;
            if (player.armor != null && !(SrcCard is 杀 && (SrcCard as 杀).IgnoreArmor)) player.armor.WhenDamaged(this);

            Debug.Log(player.posStr + "受到了" + (-Value).ToString() + "点伤害");

            // 受到伤害
            if (damageType != DamageType.Normal && player.IsLocked)
            {
                await new SetLock(player, true).Execute();
                if (!IsConDucted) conduct = true;
            }
            await base.Execute();

            // 受到伤害后
            if (player.IsAlive) await player.events.AfterDamaged.Execute(this);

            if (conduct) await Conduct();
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

            await Util.Instance.Loop(func);
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

        public List<Card> Equips { get; private set; } = new List<Card>();

        public new async Task Execute()
        {
            // 获得牌
            foreach (var card in Cards)
            {
                player.HandCards.Add(card);
                card.Src = player;
                if (card is Equipage && Dest.Equipages.ContainsValue(card as Equipage)) Equips.Add(card);
            }
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
            Debug.Log("判定结果为【" + JudgeCard.Name + JudgeCard.Suit + JudgeCard.Weight + "】");

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
            actionView?.Invoke(this);
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
            actionView?.Invoke(this);
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
            Timer.Instance.Hint = "请选择一张手牌拼点";
            if (player.team == dest.team)
            {
                card0 = (await TimerAction.SelectHandCard(player, 1))[0];
                card1 = (await TimerAction.SelectHandCard(dest, 1))[0];
            }
            else
            {
                await CompeteTimer.Instance.Run(player, dest);
                card0 = CompeteTimer.Instance.result[player];
                card1 = CompeteTimer.Instance.result[dest];
            }

            CardPile.Instance.AddToDiscard(card0);
            CardPile.Instance.AddToDiscard(card1);
            await new LoseCard(player, new List<Card> { card0 }).Execute();
            await new LoseCard(dest, new List<Card> { card1 }).Execute();

            return card0.Weight > card1.Weight;
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
                var skill = Activator.CreateInstance(Skill.SkillMap[i], player) as Skill;
                skill.Name = i;
                player.skills.Add(skill);
            }
            actionView?.Invoke(this);
        }

        public void Remove()
        {
            foreach (var i in Skills)
            {
                var skill = player.FindSkill(i);
                if (skill != null)
                {
                    skill.SetActive(false);
                    player.skills.Remove(skill);
                }
            }
            actionView?.Invoke(this);
        }
    }

    // 翻面
}