using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameCore
{
    public class CardPanelQuery
    {
        public Player player { get; private set; }
        public Player dest { get; private set; }
        private Game game => player.game;

        public string title { get; set; }
        public string hint { get; set; }
        public int second { get; private set; } = 10;

        public List<Card> cards { get; private set; }

        public CardPanelQuery(Player player, Player dest, string title, string hint, List<Card> cards)
        {
            this.player = player;
            this.dest = dest;
            this.title = title;
            this.hint = hint;
            this.cards = cards;
        }

        public CardPanelQuery(Player player, Player dest, string title, string hint, bool judgeArea)
            : this(player, dest, title, hint, (judgeArea ? dest.cards.Union(dest.JudgeCards) : dest.cards).ToList()) { }

        public async Task<List<Card>> Run()
        {
            game.eventSystem.SendToClient(new Model.CardPanelQuery
            {
                player = player.position,
                dest = dest.position,
                title = title,
                hint = hint,
                second = second,
                handCards = cards.Where(x => x.isHandCard).Select(x => x.id).ToList(),
                equipments = cards.Where(x => x is Equipment equipment && dest.Equipments.ContainsValue(equipment))
                    .Select(x => x.id)
                    .ToList(),
                judgeCards = cards.Where(x => dest.JudgeCards.Contains(x)).Select(x => x.id).ToList(),
            });

            var cts = new CancellationTokenSource();
            AutoDecision(cts.Token);

            var decisionCards = await WaitAction();
            cts.Cancel();
            cts.Dispose();

            game.eventSystem.SendToClient(new Model.FinishCardPanel { player = player.position });

            if (decisionCards.Count == 0) decisionCards.Add(cards[0]);

            return decisionCards;
        }

        public async Task<List<Card>> SelectOneCard(bool judgeArea)
        {
            var cards = dest.cards.ToList();
            if (judgeArea) cards.AddRange(dest.JudgeCards);
            return await Run();
        }

        public async Task<List<Card>> WaitAction()
        {
            var message = await game.eventSystem.PopDecision() as Model.CardDecision;
            var cards = message?.cards ?? new();
            return cards.Select(x => game.cardPile.cards[x]).ToList();
        }

        private async void AutoDecision(CancellationToken cancellationToken)
        {
            // PlayDecision decision = null;
            Card card;
            // switch (MCTS.Instance.state)
            // {
            //     case MCTS.State.Disable:
            // if (player.isSelf)
            // {
            //     if (!await new Delay(second).Run()) return;
            //     decision = new();
            //     // SendResult();
            //     // decision = new Decision();
            // }
            // else 
            if (player.isAI)
            {
                await Delay.Run(1000);
                card = cards.GetRandomOne();
                // decision = new PlayDecision { action = true, cards = AI.Shuffle(cards) };
                // SendResult(AI.Shuffle(cards), true);
                // decision = await MonteCarloTreeSearch.Instance.Run();
            }
            else
            {
                // if (!await new Delay(second).Run()) return;
                try { await Delay.Run(second * 1000, cancellationToken); }
                catch (TaskCanceledException) { return; }
                card = cards.GetRandomOne();
            }
            game.eventSystem.PushDecision(new Model.CardDecision { cards = new List<int> { card.id } });
            // break;
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
            //     case MCTS.State.Ready:
            //         if (player.isSelf)
            //         {
            //             if (!await new Delay(second).Run()) return;
            //             decision = new();
            //             // SendResult();
            //             // decision = new Decision();
            //         }
            //         else if (player.isAI)
            //         {
            //             await new Delay(1f).Run();
            //             decision = await MCTS.Instance.Run(MCTS.State.WaitCardPanel);
            //             // SendResult(AI.Shuffle(cards), true);
            //             // decision = await MonteCarloTreeSearch.Instance.Run();
            //         }
            //         break;
            //     case MCTS.State.Restoring:
            //         if (PlayDecision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitCardPanel;
            //         return;
            //     case MCTS.State.Simulating:
            //         decision = new Decision { action = true, cards = AI.Shuffle(cards) };
            //         // decision = DefaultAI();
            //         break;
            // }
            // SendResult(decision);
        }
    }
}
