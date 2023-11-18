using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCore
{
    public class CardPile : Singleton<CardPile>
    {
        // 卡牌数组
        public Card[] cards { get; private set; }
        // 牌堆
        public List<Card> RemainPile { get; } = new();
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
                var type = Type.GetType("GameCore." + i.name);
                if (type is null) continue;

                var card = Activator.CreateInstance(type) as Card;
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

        public Action<List<Card>> DiscardView;
        public Action PileCountView;
    }
}