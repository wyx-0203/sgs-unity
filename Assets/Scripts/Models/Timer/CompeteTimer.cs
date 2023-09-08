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
    public class CompeteTimer : Timer
    {
        public CompeteTimer()
        {
            players.Add(null);
            players.Add(null);

            maxCard = 1;
            minCard = 1;
            maxDest = () => 0;
            minDest = () => 0;
            isValidCard = card => players[0].HandCards.Contains(card) || players[1].HandCards.Contains(card);

            refusable = false;
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

        private Dictionary<Player, Card> result;

        public async Task<Dictionary<Player, Card>> Run(Player player0, Player player1)
        {
            currentInstance = instance;

            players[0] = player0;
            players[1] = player1;
            result = new Dictionary<Player, Card>();

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
            return result;
        }

        private async Task WaitResult()
        {
            if (!Room.Instance.IsSingle)
            {
                var message = await WebSocket.Instance.PopMessage();
                var json = JsonUtility.FromJson<TimerMessage>(message);

                Decision.list.Add(new Decision
                {
                    src = SgsMain.Instance.players[json.src],
                    action = json.action,
                    cards = json.cards.Select(x => CardPile.Instance.cards[x]).ToList(),
                });
            }

            var decision = await Decision.Pop();

            var card = decision.cards.Count == 1 ? decision.cards[0] : decision.src.HandCards[0];
            result.Add(decision.src, card);
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;

            foreach (var i in players)
            {
                if (!i.isAI) continue;
                var card = i.HandCards.OrderBy(x => -x.weight).First();
                SendDecision(new Decision { src = i, action = true, cards = new List<Card> { card } });
            }
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            foreach (var i in players)
            {
                if (i.isSelf) SendDecision(new Decision { src = i, action = true, cards = new List<Card> { i.HandCards[0] } });
            }
        }

        private new UnityAction StartTimerView => Singleton<Timer>.Instance.StartTimerView;
        private new UnityAction StopTimerView => Singleton<Timer>.Instance.StopTimerView;

        // public new void Add(Decision decision) => Singleton<Timer>.Instance.Add(decision);
        // public new async Task<Decision> Pop() => await Singleton<Timer>.Instance.Pop();

        // public static new CompeteTimer SaveInstance() => instance;
        public static new void RemoveInstance() => instance = default;
        public static void RestoreInstance(CompeteTimer _instance) => instance = _instance;
    }
}
