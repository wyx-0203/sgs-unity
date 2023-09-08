using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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
            maxDest = () => 0;
            minDest = () => 0;
            isValidCard = card => card is 无懈可击;
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

        // public Player Src { get; private set; }
        private Card scheme;

        public async Task<Decision> Run(Card scheme)
        {
            currentInstance = instance;
            this.scheme = scheme;

            StartTimerView?.Invoke();
            SelfAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            var decision = await WaitResult();

            StopTimerView?.Invoke();
            currentInstance = null;
            return decision;
        }

        private async Task<Decision> WaitResult()
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (!Room.Instance.IsSingle)
                {
                    // 若为多人模式，则等待ws通道传入消息
                    var message = await WebSocket.Instance.PopMessage();
                    var json = JsonUtility.FromJson<TimerMessage>(message);

                    Decision.list.Add(new Decision
                    {
                        src = SgsMain.Instance.players[json.src],
                        action = json.action,
                        cards = json.cards.Select(x => CardPile.Instance.cards[x]).ToList(),
                        skill = players[0].FindSkill(json.skill),
                    });
                }

                var decision = await Decision.Pop();
                if (!decision.action) continue;

                Delay.StopAll();
                return decision;
            }

            Delay.StopAll();
            return new Decision();
        }

        public new Decision AIDecision(Player player)
        {
            var card = player.FindCard<无懈可击>();
            if (card is null || scheme.Src.team == player.team) return new Decision { src = player };
            else return new Decision { src = player, action = true, cards = new List<Card> { card } };
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;

            foreach (var i in players)
            {
                if (i.isAI) SendDecision(AIDecision(i));
            }
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            foreach (var i in players)
            {
                if (i.isSelf) SendDecision(new Decision { src = i });
            }
        }

        private new UnityAction StartTimerView => Singleton<Timer>.Instance.StartTimerView;
        private new UnityAction StopTimerView => Singleton<Timer>.Instance.StopTimerView;

        // public new void Add(Decision decision) => Singleton<Timer>.Instance.Add(decision);
        // public new async Task<Decision> Pop() => await Singleton<Timer>.Instance.Pop();

        // public static new WxkjTimer SaveInstance() => instance;
        public static new void RemoveInstance() => instance = default;
        public static void RestoreInstance(WxkjTimer _instance) => instance = _instance;
    }
}