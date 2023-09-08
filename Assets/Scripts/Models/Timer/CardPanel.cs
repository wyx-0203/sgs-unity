using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class CardPanel : Singleton<CardPanel>
    {
        public Player player { get; private set; }
        public Player dest { get; private set; }

        public string Title { get; set; }
        public string Hint { get; set; }
        public int second { get; private set; } = 10;

        public List<Card> cards { get; private set; }

        public async Task<Decision> Run(Player player, Player dest, List<Card> cards)
        {
            this.player = player;
            this.dest = dest;
            this.cards = cards;

            StartTimerView?.Invoke(this);
            if (player.isSelf) SelfAutoResult();
            else if (player.isAI) AIAutoResult();
            var decision = await WaitAction();

            StopTimerView?.Invoke(this);
            Hint = "";
            Title = "";

            if (!decision.action)
            {
                decision.action = true;
                decision.cards = cards.GetRange(0, 1);
            }

            return decision;
        }

        public void SendResult(List<Card> cards, bool result)
        {
            Delay.StopAll();

            if (Room.Instance.IsSingle)
            {
                Decision.list.Add(new Decision { action = result, cards = cards });
            }
            else
            {
                var json = new TimerMessage
                {
                    msg_type = "card_panel_result",
                    action = result,
                    cards = cards.Select(x => x.id).ToList(),
                };
                WebSocket.Instance.SendMessage(json);
            }
        }

        public void SendResult()
        {
            SendResult(null, false);
        }

        public async Task<Decision> WaitAction()
        {
            if (!Room.Instance.IsSingle)
            {
                var msg = await WebSocket.Instance.PopMessage();
                var json = JsonUtility.FromJson<TimerMessage>(msg);

                Decision.list.Add(new Decision { action = json.action, cards = json.cards.Select(x => CardPile.Instance.cards[x]).ToList() });
            }

            return await Decision.Pop();
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;
            SendResult(AI.GetRandomItem(cards), true);
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;
            SendResult();
        }

        public UnityAction<CardPanel> StartTimerView;
        public UnityAction<CardPanel> StopTimerView;
    }
}
