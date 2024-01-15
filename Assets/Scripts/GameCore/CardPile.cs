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
        public List<Card> DiscardPile { get; } = new();
        // 牌堆数
        public int pileCount => RemainPile.Count;

        private static List<Model.Card> cardJsons;

        public async Task Init()
        {
            if (cardJsons is null) cardJsons = await Model.Card.GetList();

            // 初始化卡牌数组
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

            // 将所有卡牌放入弃牌堆
            DiscardPile.AddRange(cards);
            DiscardPile.RemoveAt(0);

            // 3v3模式删去闪电
            if (Mode.Instance is ThreeVSThree) DiscardPile.RemoveAll(x => x is 闪电);
        }

        /// <summary>
        /// 弹出并返回牌堆顶的牌
        /// </summary>
        public async Task<Card> Pop()
        {
            if (pileCount == 0) await Shuffle();

            var card = RemainPile[0];
            RemainPile.RemoveAt(0);

            EventSystem.Instance.Send(new Model.UpdatePileCount
            {
                count = RemainPile.Count
            });
            return card;
        }

        /// <summary>
        /// 将牌置入弃牌堆
        /// </summary>
        public void AddToDiscard(List<Card> cards, Player src)
        {
            DiscardPile.AddRange(cards);
            EventSystem.Instance.Send(new Model.AddToDiscard
            {
                player = src != null ? src.position : -1,
                cards = cards.Select(x => x.id).ToList()
            });
        }

        public void AddToDiscard(Card card, Player src)
        {
            AddToDiscard(new List<Card> { card }, src);
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
            // if (MCTS.Instance.state == MCTS.State.Restoring)
            // {
            //     if (PlayDecision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitShuffle;
            // }

            // else if (Room.Instance.IsSingle)
            // {
            // Debug.Log("shuffle");
            var discards = AI.Shuffle(DiscardPile, DiscardPile.Count);
            EventSystem.Instance.PushDecision(new Model.Shuffle { cards = discards.Select(x => x.id).ToList() });
            // PlayDecision.List.Instance.Push(new Decision { cards = discards });
            // }

            // else
            // {
            //     if (TurnSystem.Instance.CurrentPlayer.isSelf)
            //     {
            //         // 发送洗牌请求
            //         var discards = AI.Shuffle(DiscardPile, DiscardPile.Count);
            //         var json = new Decision { cards = discards }.ToMessage();
            //         WebSocket.Instance.SendMessage(json);
            //     }

            //     // 等待消息
            //     var msg = await WebSocket.Instance.PopMessage();
            //     Decision.List.Instance.Push(JsonUtilit.FromJson<Decision.Message>(msg));
            // }

            DiscardPile.Clear();
            var message = await EventSystem.Instance.PopDecision() as Model.Shuffle;
            RemainPile.AddRange(message.cards.Select(x => cards[x]));
        }
    }
}