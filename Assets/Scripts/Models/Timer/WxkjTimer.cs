using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 用于暂停主线程，并获得玩家的操作结果
    /// </summary>
    public class WxkjTimer : Timer
    {
        public WxkjTimer()
        {
            players = SgsMain.Instance.AlivePlayers;
            second = 10;

            maxCard = 1;
            minCard = 1;
            MaxDest = () => 0;
            MinDest = () => 0;
            IsValidCard = card => card is 无懈可击;
        }

        private static WxkjTimer instance;
        public static new WxkjTimer Instance
        {
            get
            {
                if (instance is null) instance = new WxkjTimer();
                return instance;
            }
        }

        public Player Src { get; private set; }
        public async Task<bool> Run()
        {
            currentInstance = instance;

            StartTimerView?.Invoke();
            SelfAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            bool result = await WaitResult();

            StopTimerView?.Invoke();

            currentInstance = null;
            return result;
        }

        private async Task<bool> WaitResult()
        {
            cards = new List<Card>();

            for (int i = 0; i < players.Count; i++)
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

                if (!json.result) continue;

                Src = SgsMain.Instance.players[json.src];
                cards.Add(CardPile.Instance.cards[json.cards[0]]);
                Delay.StopAll();
                return true;
            }

            Delay.StopAll();
            return false;
        }

        public void SendResult(int src, bool result, List<int> cards = null)
        {
            var json = new TimerMessage
            {
                msg_type = "wxkj_set_result",
                result = result,
                cards = cards,
                src = src,
            };

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else
            {
                WebSocket.Instance.SendMessage(json);
            }
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;

            foreach (var i in players)
            {
                if (i.isAI) SendResult(i.position, false, null);
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