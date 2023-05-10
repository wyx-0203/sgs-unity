using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 用于暂停主线程，并获得玩家的操作结果
    /// </summary>
    public class CompeteTimer : Timer
    {
        public CompeteTimer()
        {
            players.Add(null);
            players.Add(null);

            maxCard = 1;
            minCard = 1;
            MaxDest = () => 0;
            MinDest = () => 0;
            IsValidCard = card => players[0].HandCards.Contains(card) || players[1].HandCards.Contains(card);

            Refusable = false;
            second = 15;
        }

        private static CompeteTimer instance;
        public static new CompeteTimer Instance
        {
            get
            {
                if (instance is null) instance = new CompeteTimer();
                return instance;
            }
        }

        public Dictionary<Player, Card> result { get; private set; } = new Dictionary<Player, Card>();

        public async Task Run(Player player0, Player player1)
        {
            currentInstance = instance;

            players[0] = player0;
            players[1] = player1;
            result.Clear();

            if (player0.isSelf) await SgsMain.Instance.MoveSeat(player0);
            else if (player1.isSelf) await SgsMain.Instance.MoveSeat(player1);

            StartTimerView?.Invoke();
            SelfAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();

            await WaitResult();
            await WaitResult();

            Delay.StopAll();
            StopTimerView?.Invoke();

            currentInstance = null;
        }

        private async Task WaitResult()
        {
            TimerMessage json;
            if (Room.Instance.IsSingle)
            {
                // 若为单机模式，则通过tcs阻塞线程，等待操作结果
                waitAction = new TaskCompletionSource<TimerMessage>();
                json = await waitAction.Task;
            }
            else
            {
                // 若为多人模式，则等待ws通道传入消息
                var message = await WebSocket.Instance.PopMessage();
                json = JsonUtility.FromJson<TimerMessage>(message);
            }

            var src = SgsMain.Instance.players[json.src];
            Debug.Log(json.cards is null);
            var card = json.cards.Count == 1 ? CardPile.Instance.cards[json.cards[0]] : src.HandCards[0];
            result.Add(src, card);
        }

        public void SendResult(int src, bool result, List<int> cards = null)
        {
            var json = new TimerMessage
            {
                msg_type = "wxkj_set_result",
                result = result,
                cards = cards is null ? new List<int>() : cards,
                src = src,
            };

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else
            {
                Delay.StopAll();
                WebSocket.Instance.SendMessage(json);
            }
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;

            foreach (var i in players)
            {
                if (i.isAI) SendResult(i.position, false);
            }
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            foreach (var i in players)
            {
                if (i.isSelf) SendResult(i.position, false);
            }
        }

        private new UnityAction StartTimerView => Singleton<Timer>.Instance.StartTimerView;
        private new UnityAction StopTimerView => Singleton<Timer>.Instance.StopTimerView;
    }
}
