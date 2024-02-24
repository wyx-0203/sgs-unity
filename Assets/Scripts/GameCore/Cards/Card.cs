using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore
{
    public class Card
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int id { get; set; } = -1;
        /// <summary>
        /// 花色
        /// </summary>
        public string suit { get; set; }
        /// <summary>
        /// 点数(1~13)
        /// </summary>
        public int weight { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 卡牌名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 使用者
        /// </summary>
        public Player src { get; set; }
        public Game game => src.game;
        /// <summary>
        /// 所有目标
        /// </summary>
        public List<Player> dests { get; protected set; }
        /// <summary>
        /// 当前目标
        /// </summary>
        public Player dest { get; private set; }

        /// <summary>
        /// 使用牌
        /// </summary>
        public async Task UseCard(Player _src, List<Player> _dests = null)
        {
            src = _src;
            dests = _dests != null ? _dests : new List<Player>();
            damageOffset = new int[8];
            unmissableDests.Clear();
            invalidDests.Clear();

            try { await BeforeUse(); }
            catch (CancelUseCard) { return; }

            // 目标角色排序
            if (dests.Count > 1) dests.Sort();

            // 使用者失去此手牌
            if (!isConvert)
            {
                if (this is not Equipment) game.cardPile.AddToDiscard(this, src);
                await new LoseCard(src, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                game.cardPile.AddToDiscard(PrimiTives, src);
                await new LoseCard(src, PrimiTives).Execute();
            }

            string destStr = dests.Count > 0 ? "对" + string.Join("、", dests) : "";
            // Util.Print(src + destStr + "使用了" + this);
            game.eventSystem.SendToClient(new Model.UseCard
            {
                player = src.position,
                dests = dests.Select(x => x.position).ToList(),
                id = id,
                name = name,
                type = type,
                gender = src.general.gender,
                text = src + destStr + "使用了" + this
            });
            src.effects.ExtraDestCount.TryExecute();
            src.effects.NoTimesLimit.TryExecute();

            // 指定目标时
            await Triggered.Invoke(game, x => x.OnEveryUseCard, this);

            // 指定目标后
            foreach (var i in dests)
            {
                dest = i;
                await Triggered.Invoke(game, x => x.AfterEveryUseCard, this);
            }

            await AfterInit();

            foreach (var i in dests)
            {
                if (invalidDests.Contains(i) || i.effects.InvalidForDest.Invoke(this)) continue;
                dest = i;

                if (this is Scheme && !unmissableDests.Contains(i) && await 无懈可击.Call(this)) continue;

                await UseForeachDest();
            }
        }

        protected virtual Task BeforeUse() => Task.CompletedTask;

        protected virtual Task AfterInit() => Task.CompletedTask;

        protected virtual Task UseForeachDest() => Task.CompletedTask;



        /// <summary>
        /// 打出牌
        /// </summary>
        public async Task Put(Player player)
        {
            src = player;
            // Util.Print(player + "打出了" + this);

            // 使用者失去此手牌
            if (!isConvert)
            {
                if (!(this is Equipment)) game.cardPile.AddToDiscard(this, src);
                await new LoseCard(player, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                game.cardPile.AddToDiscard(PrimiTives, src);
                await new LoseCard(player, PrimiTives).Execute();
            }

            game.eventSystem.SendToClient(new Model.UseCard
            {
                player = src.position,
                dests = new(),
                id = id,
                name = name,
                type = type,
                gender = src.general.gender,
                text = $"{player}打出了{this}"
            });
        }

        #region 转化牌

        /// <summary>
        /// 是否为转化牌
        /// </summary>
        public bool isConvert { get; private set; } = false;

        /// <summary>
        /// 转化牌的原型
        /// </summary>
        public List<Card> PrimiTives { get; } = new();

        /// <summary>
        /// 转化牌
        /// </summary>
        /// <param name="primitives">原卡牌</param>
        /// <typeparam name="T">类型</typeparam>
        public static T Convert<T>(Player src, List<Card> primitives = null) where T : Card, new()
        {
            // 无转化牌
            if (primitives is null || primitives.Count == 0) return new T
            {
                // id为同类牌的相反数
                id = -src.game.cardPile.cards.First(x => x is T).id,
                src = src,
                isConvert = true
            };
            // 二次转化
            if (primitives[0].isConvert) return Convert<T>(src, primitives[0].PrimiTives);

            var card = new T
            {
                id = primitives.Count == 1 ? primitives[0].id : -1,
                src = src,
                isConvert = true,
                suit = primitives[0].suit,
                weight = primitives[0].weight,
            };
            card.PrimiTives.AddRange(primitives);

            foreach (var i in primitives)
            {
                if (i.suit == card.suit) continue;
                if (i.isBlack && card.isBlack) card.suit = "黑色";
                else if (i.isRed && card.isRed) card.suit = "红色";
                else
                {
                    card.suit = "";
                    break;
                }
            }

            foreach (var i in primitives)
            {
                if (i.weight != card.weight)
                {
                    card.weight = 0;
                    break;
                }
            }

            return card;
        }

        public static Card NewVirtualCard(int virtualId, Player src)
        {
            if (virtualId >= 0) return null;

            string cardName = src.game.cardPile.cards[-virtualId].name;
            var type = Type.GetType($"GameCore.{cardName}");
            var method = typeof(Card).GetMethod("Convert").MakeGenericMethod(new Type[] { type });
            return method.Invoke(null, new object[] { src, null }) as Card;
        }
        #endregion

        /// <summary>
        /// 判断此牌是否在弃牌堆
        /// </summary>
        public List<Card> InDiscardPile()
        {
            List<Card> cards = isConvert ? PrimiTives : new List<Card> { this };
            return cards.Where(x => game.cardPile.DiscardPile.Contains(x)).ToList();
        }

        protected int[] damageOffset;
        public void AddDamageValue(Player dest, int offset) => damageOffset[dest.position] += offset;

        /// <summary>
        /// 无效的目标
        /// </summary>
        public List<Player> invalidDests { get; } = new();

        /// <summary>
        /// 不可响应的目标
        /// </summary>
        public List<Player> unmissableDests { get; } = new();

        /// <summary>
        /// 是否为手牌
        /// </summary>
        public bool isHandCard => src != null && src.handCards.Contains(this);

        /// <summary>
        /// 是否可弃置
        /// </summary>
        public bool discardable => !isConvert;

        /// <summary>
        /// 是否可使用
        /// </summary>
        public bool useable => !src.effects.DisableCard.Invoke(this);

        /// <summary>
        /// 是否为T类型且可使用
        /// </summary>
        public bool Useable<T>() where T : Card => this is T && useable;

        public virtual bool IsValid() => true;
        public virtual int MaxDest() => 0;
        public virtual int MinDest() => 0;
        public virtual bool IsValidDest(Player player) => false;

        public bool isRed => suit == "红桃" || suit == "方片" || suit == "红色";
        public bool isBlack => suit == "黑桃" || suit == "草花" || suit == "黑色";

        public override string ToString()
        {
            string symbol;
            switch (suit)
            {
                case "红桃": symbol = "♥️"; break;
                case "方片": symbol = "♦️"; break;
                case "黑桃": symbol = "♠️"; break;
                case "草花": symbol = "♣️"; break;
                default: symbol = ""; break;
            }
            return "【" + name + symbol + weight + "】";
        }
    }
}