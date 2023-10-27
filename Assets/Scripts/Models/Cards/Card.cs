using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
{
    public class Card
    {
        // 编号
        public int id { get; set; } = 0;
        // 花色
        public string suit { get; set; }
        // 点数(1~13)
        public int weight { get; set; }
        // 类别
        public string type { get; set; }
        // 卡牌名
        public string name { get; set; }

        // 使用者
        public Player Src { get; set; }
        // 目标
        public List<Player> Dests { get; protected set; }

        /// <summary>
        /// 使用牌
        /// </summary>
        public async Task UseCard(Player src, List<Player> dests = null)
        {
            Src = src;
            Dests = dests != null ? dests : new List<Player>();
            damageOffset = new int[8];
            invalidDests.Clear();

            try { await BeforeUse(); }
            catch (CancelUseCard) { return; }

            // 目标角色排序
            if (Dests.Count > 1) Dests.Sort();

            string destStr = Dests.Count > 0 ? "对" + string.Join("、", Dests) : "";
            Util.Print(Src + destStr + "使用了" + this);
            if (!MCTS.Instance.isRunning)
            {
                UseCardView?.Invoke(this);
                src.effects.ExtraDestCount.TryExecute();
                src.effects.NoTimesLimit.TryExecute();
            }

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (this is not Equipment) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(Src, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                CardPile.Instance.AddToDiscard(PrimiTives);
                await new LoseCard(Src, PrimiTives).Execute();
            }

            // 指定目标时
            await EventSystem.Instance.Invoke(x => x.OnEveryUseCard, this);

            // 指定目标后
            foreach (var i in Dests)
            {
                dest = i;
                await EventSystem.Instance.Invoke(x => x.AfterUseCard, this);
            }

            await AfterInit();

            foreach (var i in Dests)
            {
                if (invalidDests.Contains(i) || i.effects.InvalidForDest.Invoke(this)) continue;
                dest = i;

                if (this is Scheme && await 无懈可击.Call(this)) continue;

                await UseForeachDest();
            }

            AfterUse?.Invoke();
        }

        protected virtual async Task BeforeUse() { await Task.Yield(); }

        protected virtual async Task AfterInit() { await Task.Yield(); }

        protected virtual async Task UseForeachDest() { await Task.Yield(); }


        public Player dest { get; private set; }

        /// <summary>
        /// 打出牌
        /// </summary>
        public async Task Put(Player player)
        {
            Src = player;
            Util.Print(player + "打出了" + this);
            if (!MCTS.Instance.isRunning) UseCardView?.Invoke(this);

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipment)) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(player, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                CardPile.Instance.AddToDiscard(PrimiTives);
                await new LoseCard(player, PrimiTives).Execute();
            }
        }

        #region 转化牌
        public bool IsConvert { get; private set; } = false;
        public List<Card> PrimiTives { get; private set; } = new();

        /// <summary>
        /// 转化牌
        /// </summary>
        /// <param name="primitives">原卡牌</param>
        /// <typeparam name="T">类型</typeparam>
        public static T Convert<T>(List<Card> primitives = null) where T : Card, new()
        {
            // 无转化牌
            if (primitives is null || primitives.Count == 0) return new T { IsConvert = true };
            // 二次转化
            if (primitives[0].IsConvert) return Convert<T>(primitives[0].PrimiTives);

            var card = new T
            {
                IsConvert = true,
                PrimiTives = primitives,
                suit = primitives[0].suit,
                weight = primitives[0].weight,
            };

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
        #endregion

        /// <summary>
        /// 判断此牌是否在弃牌堆
        /// </summary>
        public List<Card> InDiscardPile()
        {
            if (!IsConvert)
            {
                if (CardPile.Instance.DiscardPile.Contains(this)) return new List<Card> { this };
                else return new();
            }

            // if (PrimiTives.Count == 0) return null;
            return PrimiTives.Where(x => CardPile.Instance.DiscardPile.Contains(x)).ToList();
        }

        protected int[] damageOffset;
        public void AddDamageValue(Player dest, int offset) => damageOffset[dest.position] += offset;

        private List<Player> invalidDests = new();
        public void SetInvalidDest(Player dest)
        {
            if (!invalidDests.Contains(dest)) invalidDests.Add(dest);
        }

        /// <summary>
        /// 是否为手牌
        /// </summary>
        public bool isHandCard => Src != null && Src.HandCards.Contains(this);

        /// <summary>
        /// 是否可弃置
        /// </summary>
        public bool discardable => !IsConvert;

        /// <summary>
        /// 是否可使用
        /// </summary>
        public bool useable => Src is null || !Src.effects.DisableCard.Invoke(this);

        public Action AfterUse { get; set; }

        /// <summary>
        /// 是否为T类型且可使用
        /// </summary>
        // public bool Useable<T>() where T : Card => this is T && useable;

        public static UnityAction<Card> UseCardView { get; set; }

        public bool isRed => suit == "红桃" || suit == "方片" || suit == "红色";
        public bool isBlack => suit == "黑桃" || suit == "草花" || suit == "黑色";

        public override string ToString()
        {
            string symbol = "";
            switch (suit)
            {
                case "红桃": symbol = "♥️"; break;
                case "方片": symbol = "♦️"; break;
                case "黑桃": symbol = "♠️"; break;
                case "草花": symbol = "♣️"; break;
            }
            return "【" + name + symbol + weight + "】";
        }
    }
}