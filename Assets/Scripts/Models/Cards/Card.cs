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
        public int Id { get; set; } = 0;
        // 花色
        public string Suit { get; set; }
        // 点数(1~13)
        public int Weight { get; set; }
        // 类别
        public string Type { get; set; }
        // 卡牌名
        public string Name { get; set; }

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
            string cardInfo = IsConvert ? "" : "【" + Suit + Weight.ToString() + "】";
            Debug.Log(Src.posStr + "号位使用了" + Name + cardInfo);
            useCardView?.Invoke(this);

            // 目标角色排序
            if (Dests != null && Dests.Count > 1)
            {
                TurnSystem.Instance.SortDest(Dests);
            }

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipage)) CardPile.Instance.AddToDiscard(this);
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
        }

        public bool Disabled(Player dest) => dest.DisableForMe(this);

        /// <summary>
        /// 打出牌
        /// </summary>
        public async Task Put(Player player)
        {
            Src = player;
            string cardInfo = IsConvert ? "" : "【" + Suit + Weight.ToString() + "】";
            Debug.Log(player.posStr + "号位打出了" + Name + cardInfo);
            useCardView?.Invoke(this);

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipage)) CardPile.Instance.AddToDiscard(this);
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
        public List<Card> PrimiTives { get; private set; } = new List<Card>();

        /// <summary>
        /// 转化牌
        /// </summary>
        /// <param name="primitives">原卡牌</param>
        /// <typeparam name="T">类型</typeparam>
        public static T Convert<T>(List<Card> primitives = null) where T : Card, new()
        {
            if (primitives is null) primitives = new List<Card>();
            // 二次转化
            if (primitives.Count > 0 && primitives[0].IsConvert) return Convert<T>(primitives[0].PrimiTives);

            var card = new T();
            card.IsConvert = true;
            card.PrimiTives = primitives;

            if (primitives.Count == 0) return card;

            card.Suit = primitives[0].Suit;
            card.Weight = primitives[0].Weight;

            foreach (var i in primitives)
            {
                if (i.Suit == card.Suit) continue;
                if (i.Suit == "黑桃" || i.Suit == "草花")
                {
                    if (card.Suit == "黑桃" || card.Suit == "草花" || card.Suit == "黑色") card.Suit = "黑色";
                    else
                    {
                        card.Suit = "无花色";
                        break;
                    }
                }
                else
                {
                    if (card.Suit == "红桃" || card.Suit == "方片" || card.Suit == "红色") card.Suit = "红色";
                    else
                    {
                        card.Suit = "无花色";
                        break;
                    }
                }
            }

            foreach (var i in primitives)
            {
                if (i.Weight != card.Weight)
                {
                    card.Weight = 0;
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
                if (CardPile.Instance.discardPile.Contains(this)) return new List<Card> { this };
                else return null;
            }

            if (PrimiTives.Count == 0) return null;

            // var list = new List<Card>();
            return PrimiTives.Where(x => CardPile.Instance.discardPile.Contains(x)).ToList();
            // foreach (var i in PrimiTives)
            // {
            //     if (CardPile.Instance.discardPile.Contains(i)) list.Add(i);
            // }
            // return list;
        }

        public virtual bool AIPerform()
        {
            if (!CardArea.Instance.ValidCard(this)) return false;
            // Debug.Log("a");

            Operation.Instance.Cards.Add(this);

            // foreach (var i in AI.Instance.DestList)
            // {
            //     // Debug.Log("a");
            //     if (DestArea.Instance.ValidDest(i))
            //     {
            //         Operation.Instance.Dests.Add(i);
            //     }
            //     if (Operation.Instance.Dests.Count == DestArea.Instance.MaxDest()) break;
            // }

            // return Operation.Instance.AICommit();

            return AI.Instance.SelectDest();

            // if (Operation.Instance.Dests.Count >= DestArea.Instance.MinDest())
            // {
            //     Operation.Instance.AICommit();
            //     Operation.Instance.Clear();
            //     return true;
            // }
            // Operation.Instance.Clear();
            // return false;
        }

        private static UnityAction<Card> useCardView;
        public static event UnityAction<Card> UseCardView
        {
            add => useCardView += value;
            remove => useCardView -= value;
        }
    }
}