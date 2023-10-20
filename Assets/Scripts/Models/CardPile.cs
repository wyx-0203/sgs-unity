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
        // 卡牌数组
        public Card[] cards { get; private set; }
        // 牌堆
        public List<Card> RemainPile { get; private set; } = new();
        // 弃牌堆
        public List<Card> DiscardPile { get; private set; }
        // 牌堆数
        public int PileCount => RemainPile.Count;

        private static List<CardJson> cardJsons;

        public async Task Init()
        {
            if (!MCTS.Instance.isRunning)
            {
                string url = Url.JSON + "card.json";
                cardJsons = JsonList<CardJson>.FromJson(await WebRequest.Get(url));
            }
            cards = new Card[cardJsons.Count];

            foreach (var i in cardJsons)
            {
                if (!cardMap.ContainsKey(i.name)) continue;

                var card = Activator.CreateInstance(cardMap[i.name]) as Card;
                card.id = i.id;
                card.suit = i.suit;
                card.weight = i.weight;
                card.type = i.type;
                card.name = i.name;

                cards[i.id] = card;
            }

            DiscardPile = new List<Card>(cards);
            DiscardPile.RemoveAt(0);

            if (Mode.Instance is ThreeVSThree) RemainPile.RemoveAll(x => x is 闪电);
        }

        /// <summary>
        /// 弹出并返回牌堆顶的牌
        /// </summary>
        public async Task<Card> Pop()
        {
            if (PileCount == 0) await Shuffle();

            var card = RemainPile[0];
            RemainPile.RemoveAt(0);

            PileCountView?.Invoke();
            return card;
        }

        /// <summary>
        /// 将牌置入弃牌堆
        /// </summary>
        public void AddToDiscard(List<Card> cards)
        {
            DiscardPile.AddRange(cards);
            DiscardView?.Invoke(cards);
        }

        public void AddToDiscard(Card card)
        {
            AddToDiscard(new List<Card> { card });
        }

        public void RemoveToDiscard(Card card)
        {
            DiscardPile.Remove(card);
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private async Task Shuffle()
        {
            if (MCTS.Instance.state == MCTS.State.Restoring)
            {
                if (Decision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitShuffle;
            }

            else if (Room.Instance.IsSingle)
            {
                // Debug.Log("shuffle");
                var discards = AI.Shuffle(DiscardPile, DiscardPile.Count);
                Decision.List.Instance.Push(new Decision { cards = discards });
            }

            else
            {
                if (TurnSystem.Instance.CurrentPlayer.isSelf)
                {
                    // 发送洗牌请求
                    var discards = AI.Shuffle(DiscardPile, DiscardPile.Count);
                    var json = new Decision { cards = discards }.ToMessage();
                    WebSocket.Instance.SendMessage(json);
                }

                // 等待消息
                var msg = await WebSocket.Instance.PopMessage();
                Decision.List.Instance.Push(JsonUtility.FromJson<Decision.Message>(msg));
            }

            DiscardPile.Clear();
            RemainPile.AddRange((await Decision.List.Instance.Pop()).cards);
        }

        public UnityAction<List<Card>> DiscardView { get; set; }
        public UnityAction PileCountView { get; set; }

        private Dictionary<string, System.Type> cardMap = new Dictionary<string, System.Type>
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
    }
}