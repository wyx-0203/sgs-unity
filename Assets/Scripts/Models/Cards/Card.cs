using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Linq;

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
        public virtual async Task UseCard(Player src, List<Player> dests = null)
        {
            Src = src;
            Dests = dests;
            Debug.Log(Src + "使用了" + this);
            UseCardView?.Invoke(this);

            // 目标角色排序
            if (Dests != null && Dests.Count > 1)
            {
                TurnSystem.Instance.SortDest(Dests);
            }

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipment)) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(Src, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                CardPile.Instance.AddToDiscard(PrimiTives);
                await new LoseCard(Src, PrimiTives).Execute();
            }

            // 指定目标时
            await Src.events.WhenUseCard.Execute(this);
            // 指定目标后
            await Src.events.AfterUseCard.Execute(this);

            if (dests != null) foreach (var i in new List<Player>(Dests)) if (i.DisableForMe(this)) Dests.Remove(i);
        }

        /// <summary>
        /// 打出牌
        /// </summary>
        public async Task Put(Player player)
        {
            Src = player;
            string cardInfo = IsConvert ? "" : "【" + suit + weight.ToString() + "】";
            Debug.Log(player.posStr + "号位打出了" + name + cardInfo);
            UseCardView?.Invoke(this);

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
                else return null;
            }

            if (PrimiTives.Count == 0) return null;
            return PrimiTives.Where(x => CardPile.Instance.DiscardPile.Contains(x)).ToList();
        }

        public static UnityAction<Card> UseCardView { get; set; }

        public bool isRed => suit == "红桃" || suit == "方片" || suit == "红色";
        public bool isBlack => suit == "黑桃" || suit == "草花" || suit == "黑色";

        public override string ToString()
        {
            string suitSymbol = "";
            switch (suit)
            {
                case "红桃": suitSymbol = "♥️"; break;
                case "方片": suitSymbol = "♦️"; break;
                case "黑桃": suitSymbol = "♠️"; break;
                case "梅花": suitSymbol = "♣️"; break;
            }
            return "【" + name + suitSymbol + weight + "】";
        }
    }
}