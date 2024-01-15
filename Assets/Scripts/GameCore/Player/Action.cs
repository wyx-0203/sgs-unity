using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Message = Model.Message;
using DamageType = Model.Damage.Type;

namespace GameCore
{
    public abstract class PlayerAction
    {
        public Player player { get; private set; }
        public PlayerAction(Player player)
        {
            this.player = player;
        }


        public abstract Task Execute();

        public void SendMessage(Message message)
        {
            message.player = player.position;
            EventSystem.Instance.Send(message);
        }

        // protected abstract ActionMessage GetMessage();
    }

    /// <summary>
    /// 获得牌
    /// </summary>
    public abstract class GetCard : PlayerAction
    {
        /// <summary>
        /// 获得牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="cards">卡牌数组</param>
        public GetCard(Player player, List<Card> cards) : base(player) => Cards = cards;
        public List<Card> Cards { get; protected set; }

        protected void Add()
        {
            // 获得牌
            player.handCards.AddRange(Cards);
            foreach (var i in Cards) i.src = player;
        }
    }

    /// <summary>
    /// 摸牌
    /// </summary>
    public class DrawCard : GetCard
    {
        /// <summary>
        /// 摸牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="count">摸牌数</param>
        public DrawCard(Player player, int count) : base(player, new List<Card>()) => Count = count;
        public int Count { get; set; }

        public override async Task Execute()
        {
            await Triggered.Invoke(x => x.BeforeEveryDrawCard, this);
            // await EventSystem.Instance.OnGetCardFromPile(this);

            if (Count == 0) return;
            Util.Print(player + "摸了" + Count + "张牌");
            // 摸牌
            for (int i = 0; i < Count; i++) Cards.Add(await CardPile.Instance.Pop());
            // 获得牌
            Add();

            SendMessage(new Model.DrawCard
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = player.handCardsCount
            });

            // 执行获得牌后事件
            await Triggered.Invoke(x => x.OnEveryDrawCard, this);
            // await EventSystem.Instance.AfterGetCard(this);
            // await base.Execute();

            // 鲁肃
            if (afterExecute != null) await afterExecute();
        }

        public bool inGetPhase { get; set; } = false;

        public Func<Task> afterExecute;
    }

    public class GetDiscard : GetCard
    {
        public GetDiscard(Player player, List<Card> cards) : base(player, cards) { }

        public override async Task Execute()
        {
            if (Cards.Count == 0) return;

            Util.Print(player + "从弃牌堆获得了" + string.Join("、", Cards));
            foreach (var i in Cards) CardPile.Instance.DiscardPile.Remove(i);
            // await base.Execute();
            Add();
            SendMessage(new Model.GetDiscard
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = player.handCardsCount
            });
            await Triggered.Invoke(x => x.OnEveryGetCard, this);
        }
    }

    public class GetJudgeCard : GetCard
    {
        public GetJudgeCard(Player player, Card card) : base(player, new List<Card> { card }) { }

        public override async Task Execute()
        {
            var card = Cards[0];

            (card as DelayScheme).RemoveToJudgeArea();
            Util.Print(player + "获得了" + string.Join("、", Cards));
            // await base.Execute();
            Add();
            SendMessage(new Model.GetCardInJudgeArea
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = player.handCardsCount
            });
            await Triggered.Invoke(x => x.OnEveryGetCard, this);
        }
    }

    /// <summary>
    /// 失去牌
    /// </summary>
    public class LoseCard : PlayerAction
    {
        /// <summary>
        /// 失去牌
        /// </summary>
        public LoseCard(Player player, List<Card> cards) : base(player) => Cards = cards;
        public List<Card> Cards { get; private set; }

        public async Task Remove()
        {
            foreach (var card in Cards)
            {
                if (player.handCards.Contains(card)) player.handCards.Remove(card);
                else if (card is Equipment equipage) await equipage.Remove();
            }
        }

        public override async Task Execute()
        {
            if (Cards.Count == 0) return;
            await Remove();
            SendMessage(new Model.LoseCard
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = player.handCardsCount
            });

            // 执行失去牌后事件
            await Triggered.Invoke(x => x.OnEveryLoseCard, this);
            // await EventSystem.Instance.AfterLoseCard(this);
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

        public override async Task Execute()
        {
            if (Cards is null || Cards.Count == 0) return;
            Util.Print(player + "弃置了" + string.Join("、", Cards));

            CardPile.Instance.AddToDiscard(Cards, player);

            // await base.Execute();
            await Remove();
            SendMessage(new Model.LoseCard
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = player.handCardsCount
            });

            await Triggered.Invoke(x => x.OnEveryDiscard, this);
        }
    }

    public abstract class UpdateHp : PlayerAction
    {
        /// <summary>
        /// 改变体力
        /// </summary>
        public UpdateHp(Player player, int value) : base(player)
        {
            this.value = value;
        }
        public int value { get; set; }

        public void SetValue()
        {
            // 更新体力
            player.hp += value;
            // SendMessage();

            // // 失去体力
            // // if (value < 0 && this is not Damaged)
            // // {
            // //     Util.Print(player + "失去了" + (-value) + "点体力");
            // // }

            // // // 濒死
            // // if (player.hp < 1 && value < 0) await NearDeath();

            // // 改变体力后事件
            // // await EventSystem.Instance.OnUpdateHp(this);
            // await Triggered.Invoke(x => x.OnEveryUpdateHp, this);
        }

        protected async Task NearDeath()
        {
            if (player.hp > 0) return;
            Util.Print(player + "进入濒死状态");

            foreach (var i in Game.Instance.AlivePlayers.OrderBy(x => x.orderKey))
            {
                await Triggered.Invoke(x => x.OnEveryNearDeath, player);
                if (player.hp >= 1) return;
                while (await 桃.Call(i, player))
                {
                    if (player.hp >= 1) return;
                }
            }

            await new Die(player, this is Damage damaged && damaged.Src != player ? damaged.Src : null).Execute();
        }
    }

    /// <summary>
    /// 阵亡
    /// </summary>
    public class Die : PlayerAction
    {
        public Die(Player player, Player damageSrc) : base(player)
        {
            DamageSrc = damageSrc;
        }

        public Player DamageSrc { get; private set; }

        public override async Task Execute()
        {
            SendMessage(new Model.Die
            {
                damageSrc = DamageSrc != null ? DamageSrc.position : -1
            });

            // 清除技能
            while (player.skills.Count > 0) player.skills[0].Remove();

            // 解锁
            if (player.locked) await new SetLock(player).Execute();

            // 修改位置关系
            player.alive = false;
            player.next.last = player.last;
            player.last.next = player.next;
            player.teammates.Remove(player);
            Game.Instance.AlivePlayers.Remove(player);

            // 弃置所有牌
            await new Discard(player, player.cards.ToList()).Execute();

            // 清空判定区
            CardPile.Instance.AddToDiscard(player.JudgeCards.Cast<Card>().ToList(), player);
            foreach (var i in player.JudgeCards.ToList()) i.RemoveToJudgeArea();

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

        public override async Task Execute()
        {
            // 判断体力是否超过上限
            int t = player.hp + value - player.hpLimit;
            if (t > 0)
            {
                value -= t;
                if (value == 0) return;
            }

            Util.Print(player + "回复了" + value + "点体力");

            // 回复体力
            // await base.Execute();
            SetValue();
            SendMessage(new Model.Recover
            {
                value = value,
                hp = player.hp,
                handCardsLimit = player.handCardsCount
            });

            // 执行事件
            await Triggered.Invoke(x => x.OnEveryRecover, this);
            // await EventSystem.Instance.
        }
    }

    public class LoseHp : UpdateHp
    {
        public LoseHp(Player player, int value) : base(player, -value) { }
        public new int value { get => -base.value; set => base.value = -value; }

        public override async Task Execute()
        {
            Util.Print(player + "失去了" + value + "点体力");
            SetValue();
            SendMessage(new Model.LoseHp
            {
                value = -value,
                hp = player.hp,
                handCardsLimit = player.handCardsCount
            });
            await NearDeath();
            await Triggered.Invoke(x => x.OnEveryLoseHp, this);
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public class Damage : UpdateHp
    {
        public Damage(Player player, Player src, Card srcCard = null, int value = 1, DamageType type = DamageType.Normal)
            : base(player, -value)
        {
            Src = src;
            SrcCard = srcCard;
            this.type = type;
        }

        public Player Src { get; private set; }
        public Card SrcCard { get; private set; }
        public DamageType type { get; private set; }
        public bool isConDucted { get; set; } = false;
        private bool conduct = false;
        public new int value { get => -base.value; set => base.value = -value; }

        public override async Task Execute()
        {
            // 受到伤害时
            try { await Triggered.Invoke(x => x.BeforeEveryDamaged, this); }
            catch (PreventDamage) { return; }

            // 藤甲 义绝
            value += player.effects.OffsetDamageValue.Invoke(this);
            player.effects.OffsetDamageValue.TryExecute();

            // 白银狮子
            if (value > 1 && player.armor is 白银狮子 bysz)
            {
                value = 1;
                bysz.Execute();
            }
            Util.Print(player + "受到了" + value + "点伤害");

            // 解锁
            if (type != DamageType.Normal && player.locked)
            {
                await new SetLock(player, true).Execute();
                if (!isConDucted) conduct = true;
            }

            try
            {
                // 受到伤害
                // await base.Execute();
                SetValue();
                SendMessage(new Model.Damage
                {
                    value = -value,
                    hp = player.hp,
                    handCardsLimit = player.handCardsCount,
                    type = type,
                    src = Src != null ? Src.position : -1
                });
                await NearDeath();
                await Triggered.Invoke(x => x.OnEveryDamaged, this);
            }
            catch (PlayerDie) { }

            // 铁锁传导
            if (conduct) await Conduct();
        }

        /// <summary>
        /// 铁索连环传导
        /// </summary>
        private async Task Conduct()
        {
            foreach (var i in Game.Instance.AlivePlayers.Where(x => x.locked).OrderBy(x => x.orderKey))
            {
                var damaged = new Damage(i, Src, SrcCard, value, type);
                damaged.isConDucted = true;
                await damaged.Execute();
            }
        }
    }

    /// <summary>
    /// 获得其他角色的牌
    /// </summary>
    public class GetAnothersCard : GetCard
    {
        public GetAnothersCard(Player player, Player dest, List<Card> cards) : base(player, cards)
        {
            this.dest = dest;
            Cards = cards;
        }
        public Player dest { get; private set; }

        public override async Task Execute()
        {
            var known = Cards.Select(x => !x.isHandCard).ToList();

            // 目标失去牌
            var loseCard = new LoseCard(dest, Cards);
            await loseCard.Remove();
            loseCard.SendMessage(new Model.LoseCard
            {
                cards = Cards.Select(x => x.id).ToList(),
                handCardsCount = dest.handCardsCount
            });

            // 获得牌
            Add();
            Util.Print(player + "获得了" + dest + "的" + Cards.Count + "张牌");
            SendMessage(new Model.GetAnothersCard
            {
                cards = Cards.Select(x => x.id).ToList(),
                dest = dest.position,
                handCardsCount = player.handCardsCount,
                known = known
            });

            // 执行获得牌后事件
            await Triggered.Invoke(x => x.OnEveryLoseCard, loseCard);
            await Triggered.Invoke(x => x.OnEveryGetAnothersCard, this);
        }
    }

    /// <summary>
    /// 判定
    /// </summary>
    public class Judge
    {
        public static async Task<Card> Execute()
        {
            var JudgeCard = await CardPile.Instance.Pop();
            CardPile.Instance.AddToDiscard(JudgeCard, null);
            Util.Print("判定结果为" + JudgeCard);

            return JudgeCard;
        }
    }

    /// <summary>
    /// 展示手牌
    /// </summary>
    public class ShowCard : PlayerAction
    {
        public ShowCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; protected set; }

        public override async Task Execute()
        {
            await Task.Yield();
            Util.Print(player + "展示了" + string.Join("、", Cards));
            SendMessage(new Model.ShowCard
            {
                cards = Cards.Select(x => x.id).ToList(),
            });
        }
    }

    /// <summary>
    /// 横置 (重置)
    /// </summary>
    public class SetLock : PlayerAction
    {
        public SetLock(Player player, bool byDamage = false) : base(player)
        {
            ByDamage = byDamage;
        }

        public bool ByDamage { get; private set; }

        public override async Task Execute()
        {
            player.locked = !player.locked;
            SendMessage(new Model.SetLock
            {
                value = player.locked,
                byDamage = ByDamage
            });
            await Task.Yield();
        }
    }

    /// <summary>
    /// 获得或失去技能
    /// </summary>
    public class AddSkill : PlayerAction
    {
        public AddSkill(Player player, List<string> skills) : base(player)
        {
            Skills = skills;
        }
        public List<string> Skills { get; protected set; }

        public override async Task Execute()
        {
            foreach (var i in Skills) Skill.New(i, player);
            SendMessage(new Model.UpdateSkill
            {
                skills = player.GetSkillModels()
            });
            await Task.Yield();
        }


        // public void Remove()
        // {
        //     foreach (var i in Skills)
        //     {
        //         var skill = player.FindSkill(i);
        //         if (skill != null) skill.Remove();
        //     }
        // }
    }

    /// <summary>
    /// 获得或失去技能
    /// </summary>
    public class RemoveSkill : PlayerAction
    {
        public RemoveSkill(Player player, List<string> skills) : base(player)
        {
            Skills = skills;
        }
        public List<string> Skills { get; protected set; }

        // public async Task Execute()
        // {
        //     foreach (var i in Skills) await Skill.New(i, player);
        //     SendMessage();
        // }

        public override async Task Execute()
        {
            await Task.Yield();
            foreach (var i in Skills)
            {
                var skill = player.FindSkill(i);
                if (skill != null) skill.Remove();
            }
            SendMessage(new Model.UpdateSkill
            {
                skills = player.GetSkillModels()
            });
        }
    }

    public class ExChange : PlayerAction
    {
        public ExChange(Player player, Player dest) : base(player)
        {
            this.dest = dest;
        }

        public Player dest { get; private set; }

        public override async Task Execute()
        {
            var cards0 = new List<Card>(player.handCards);
            var cards1 = new List<Card>(dest.handCards);
            // await new LoseCard(player, cards0).Execute();
            // await new LoseCard(Dest, cards1).Execute();
            await new GetAnothersCard(player, dest, cards1).Execute();
            await new GetAnothersCard(dest, player, cards0).Execute();
        }
    }

    // 翻面
    public class TurnOver : PlayerAction
    {
        public TurnOver(Player player) : base(player) { }

        public override async Task Execute()
        {
            player.isTurnOver = !player.isTurnOver;
            SendMessage(new Model.TurnOver
            {
                value = player.isTurnOver
            });
            // await EventSystem.Instance.OnTurnOver(this);
            await Triggered.Invoke(x => x.OnEveryTurnOver, this);
        }

    }
}