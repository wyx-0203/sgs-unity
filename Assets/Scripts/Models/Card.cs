using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class Card
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int id { get; set; }
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
        /// <summary>
        /// 所有目标
        /// </summary>
        // public List<Player> dests { get; protected set; }
        // /// <summary>
        // /// 当前目标
        // /// </summary>
        // public Player dest { get; private set; }




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
            if (primitives is null || primitives.Count == 0) return new T { src = src, isConvert = true };
            // 二次转化
            if (primitives[0].isConvert) return Convert<T>(src, primitives[0].PrimiTives);

            var card = new T
            {
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
        #endregion

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
        // public bool useable => !src.effects.DisableCard.Invoke(this);

        /// <summary>
        /// 是否为T类型且可使用
        /// </summary>
        // public bool Useable<T>() where T : Card => this is T && useable;


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