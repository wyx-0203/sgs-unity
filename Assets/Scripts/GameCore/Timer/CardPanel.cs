using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCore
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
            await AutoDecision();
            var decision = await WaitAction();

            StopTimerView?.Invoke(this);
            Hint = "";
            Title = "";

            if (!decision.action)
            {
                decision.action = true;
                decision.cards.Add(cards[0]);
            }

            return decision;
        }

        public void SendResult(Decision decision)
        {
            Delay.StopAll();

            if (Room.Instance.IsSingle)
            {
                Decision.List.Instance.Push(decision);
            }
            else
            {
                // var json = new Decision.Message
                // {
                //     msg_type = "card_panel_result",
                //     action = decision.action,
                //     cards = decision.cards.Select(x => x.id).ToList(),
                // };
                WebSocket.Instance.SendMessage(decision.ToMessage());
            }
        }

        public void SendResult(List<Card> cards)
        {
            SendResult(new Decision { action = true, cards = cards });
        }

        public async Task<Decision> WaitAction()
        {
            if (!Room.Instance.IsSingle)
            {
                var msg = await WebSocket.Instance.PopMessage();
                var json = JsonUtility.FromJson<Decision.Message>(msg);

                Decision.List.Instance.Push(json);
            }

            return await Decision.List.Instance.Pop();
        }

        private async Task AutoDecision()
        {
            Decision decision = null;
            switch (MCTS.Instance.state)
            {
                case MCTS.State.Disable:
                    if (player.isSelf)
                    {
                        if (!await new Delay(second).Run()) return;
                        decision = new();
                        // SendResult();
                        // decision = new Decision();
                    }
                    else if (player.isAI)
                    {
                        await new Delay(1f).Run();
                        decision = new Decision { action = true, cards = AI.Shuffle(cards) };
                        // SendResult(AI.Shuffle(cards), true);
                        // decision = await MonteCarloTreeSearch.Instance.Run();
                    }
                    break;
                // if (players[0].isSelf)
                // {
                //     if (!await new Delay(second).Run()) return;
                //     decision = new Decision();
                // }
                // else if(players[0].isAI)
                // {
                //     await new Delay(1f).Run();
                //     decision = DefaultAI();
                // }
                // break;
                case MCTS.State.Ready:
                    if (player.isSelf)
                    {
                        if (!await new Delay(second).Run()) return;
                        decision = new();
                        // SendResult();
                        // decision = new Decision();
                    }
                    else if (player.isAI)
                    {
                        await new Delay(1f).Run();
                        decision = await MCTS.Instance.Run(MCTS.State.WaitCardPanel);
                        // SendResult(AI.Shuffle(cards), true);
                        // decision = await MonteCarloTreeSearch.Instance.Run();
                    }
                    break;
                case MCTS.State.Restoring:
                    if (Decision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitCardPanel;
                    return;
                case MCTS.State.Simulating:
                    decision = new Decision { action = true, cards = AI.Shuffle(cards) };
                    // decision = DefaultAI();
                    break;
            }
            SendResult(decision);
        }

        public Action<CardPanel> StartTimerView;
        public Action<CardPanel> StopTimerView;
    }
}
