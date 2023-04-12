using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Model
{
    public class CardPile : Singleton<CardPile>
    {
        public async Task Init()
        {
            string url = Url.JSON + "card.json";
            List<CardJson> cardJsons = JsonList<CardJson>.FromJson(await WebRequest.Get(url));

            cards = new List<Card>();
            remainPile = new List<Card>();
            discardPile = new List<Card>();
            foreach (var cardJson in cardJsons)
            {
                Card card;
                if (cardMap.ContainsKey(cardJson.name))
                {
                    card = Activator.CreateInstance(cardMap[cardJson.name]) as Card;
                }
                else card = new 桃();

                card.Id = cardJson.id;
                card.Suit = cardJson.suit;
                card.Weight = cardJson.weight;
                card.Type = cardJson.type;
                card.Name = cardJson.name;

                cards.Add(card);
                discardPile.Add(card);
            }

            discardPile.RemoveAt(0);
            await Shuffle();
        }

        public List<Card> cards;

        public Dictionary<string, System.Type> cardMap = new Dictionary<string, System.Type>
        {
            { "杀", typeof(杀) },
            { "闪", typeof(闪) },
            { "桃", typeof(桃) },
            { "火杀", typeof(火杀) },
            { "雷杀", typeof(雷杀) },
            { "酒", typeof(酒) },

            { "绝影", typeof(PlusHorse) },
            { "大宛", typeof(SubHorse) },
            { "赤兔", typeof(SubHorse) },
            { "爪黄飞电", typeof(PlusHorse) },
            { "的卢", typeof(PlusHorse) },
            { "紫骍", typeof(SubHorse) },
            { "骅骝", typeof(PlusHorse) },

            { "青龙偃月刀", typeof(青龙偃月刀) },
            { "麒麟弓", typeof(麒麟弓) },
            { "雌雄双股剑", typeof(雌雄双股剑) },
            { "青釭剑", typeof(青釭剑) },
            { "丈八蛇矛", typeof(丈八蛇矛) },
            { "诸葛连弩", typeof(诸葛连弩) },
            { "贯石斧", typeof(贯石斧) },
            { "方天画戟", typeof(方天画戟) },
            { "朱雀羽扇", typeof(朱雀羽扇) },
            { "古锭刀", typeof(古锭刀) },
            { "寒冰剑", typeof(寒冰剑) },

            { "八卦阵", typeof(八卦阵) },
            { "藤甲", typeof(藤甲) },
            { "仁王盾", typeof(仁王盾) },
            { "白银狮子", typeof(白银狮子) },

            { "乐不思蜀", typeof(乐不思蜀) },
            { "兵粮寸断", typeof(兵粮寸断) },
            { "闪电", typeof(闪电) },

            { "过河拆桥", typeof(过河拆桥) },
            { "顺手牵羊", typeof(顺手牵羊) },
            { "无懈可击", typeof(无懈可击) },
            { "南蛮入侵", typeof(南蛮入侵) },
            { "万箭齐发", typeof(万箭齐发) },
            { "桃园结义", typeof(桃园结义) },
            { "无中生有", typeof(无中生有) },
            { "决斗", typeof(决斗) },
            { "借刀杀人", typeof(借刀杀人) },
            { "铁索连环", typeof(铁索连环) },
            { "火攻", typeof(火攻) },
        };

        // 牌堆
        private List<Card> remainPile;
        // 弃牌堆
        public List<Card> discardPile;

        // 牌堆数
        public int PileCount { get => remainPile.Count; }


        /// <summary>
        /// 弹出并返回牌堆顶的牌
        /// </summary>
        public async Task<Card> Pop()
        {
            Card T = remainPile[0];
            remainPile.RemoveAt(0);

            if (remainPile.Count == 0) await Shuffle();
            PileCountView?.Invoke(this);

            return T;
        }

        /// <summary>
        /// 将一张牌添加到弃牌堆
        /// </summary>
        public void AddToDiscard(List<Card> cards)
        {
            DiscardView?.Invoke(cards);
            foreach (var card in cards) discardPile.Add(card);
        }

        public void AddToDiscard(Card card)
        {
            AddToDiscard(new List<Card> { card });
        }

        public void RemoveToDiscard(Card card)
        {
            discardPile.Remove(card);
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private async Task Shuffle()
        {
            List<int> cardIds = discardPile.Select(x => x.Id).ToList();

            if (Room.Instance.IsSingle || TurnSystem.Instance.CurrentPlayer.isSelf)
            {
                // 随机取一个元素与第i个元素交换
                for (int i = 0; i < cardIds.Count; i++)
                {
                    int t = UnityEngine.Random.Range(i, cardIds.Count);
                    var card = cardIds[i];
                    cardIds[i] = cardIds[t];
                    cardIds[t] = card;
                }

                // 发送洗牌请求
                if (!Room.Instance.IsSingle)
                {
                    var json = new ShuffleMessage
                    {
                        msg_type = "shuffle",
                        cards = cardIds,
                    };
                    WS.Instance.SendJson(json);
                }
            }

            // 等待消息
            if (!Room.Instance.IsSingle)
            {
                var msg = await WS.Instance.PopMsg();
                cardIds = JsonUtility.FromJson<ShuffleMessage>(msg).cards;
            }

            discardPile.Clear();
            remainPile = cardIds.Select(x => cards[x]).ToList();
        }

        public UnityAction<List<Card>> DiscardView { get; set; }
        public UnityAction<CardPile> PileCountView { get; set; }
    }
}