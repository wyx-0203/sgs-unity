using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class CardPanel : Singleton<CardPanel>
    {
        private TaskCompletionSource<TimerMessage> waitAction;

        public Player player { get; private set; }
        public Player dest { get; private set; }
        public TimerType timerType { get; private set; }
        public bool judgeArea { get; private set; }
        public bool display { get; set; } = false;

        public string Title { get; set; }
        public string Hint { get; set; }
        public int second { get; private set; } = 10;

        public List<Card> Cards { get; private set; }

        public async Task<bool> Run(Player player, Player dest, TimerType timerType, bool judgeArea = false)
        {
            this.player = player;
            this.dest = dest;
            this.timerType = timerType;
            this.judgeArea = judgeArea;

            if (GameOver.Instance.Check()) return false;

            StartTimerView?.Invoke(this);
            if (player.isSelf) SelfAutoResult();
            else if (player.isAI) AIAutoResult();
            var result = await WaitAction();
            StopTimerView?.Invoke(this);

            Hint = "";
            Title = "";
            display = false;

            return result;
        }

        public void SetResult(List<int> cards)
        {
            Cards = cards.Select(x => CardPile.Instance.cards[x]).ToList();
        }

        public void SendResult(List<int> cards, bool result)
        {
            Delay.StopAll();

            var json = new TimerMessage
            {
                msg_type = "card_panel_result",
                result = result,
                cards = cards,
            };

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else WS.Instance.SendJson(json);
        }

        public void SendResult()
        {
            SendResult(null, false);
        }

        public async Task<bool> WaitAction()
        {
            if (GameOver.Instance.Check()) return false;
            TimerMessage json;
            if (Room.Instance.IsSingle)
            {
                waitAction = new TaskCompletionSource<TimerMessage>();
                json = await waitAction.Task;
            }
            else
            {
                var msg = await WS.Instance.PopMsg();
                json = JsonUtility.FromJson<TimerMessage>(msg);
            }


            if (json.result) SetResult(json.cards);
            return json.result;
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;
            SendResult();
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;
            SendResult();
        }

        public async Task<Card> SelectCard(Player player, Player dest, bool judgeArea = false)
        {
            if (player.teammate == dest) display = true;
            bool result = await Run(player, dest, TimerType.区域内, judgeArea);
            Card card;
            if (!result)
            {
                // var l=dest.Equipages.Values.Select(x=>x!=null);
                // if(l.Count()>0)
                if (dest.armor != null) card = dest.armor;
                else if (dest.plusHorse != null) card = dest.plusHorse;
                else if (dest.weapon != null) card = dest.weapon;
                else if (dest.subHorse != null) card = dest.subHorse;
                else if (dest.HandCardCount != 0) card = dest.HandCards[0];
                else card = dest.JudgeArea[0];
            }
            else card = CardPanel.Instance.Cards[0];
            return card;
        }

        public UnityAction<CardPanel> StartTimerView;
        public UnityAction<CardPanel> StopTimerView;
    }
}
