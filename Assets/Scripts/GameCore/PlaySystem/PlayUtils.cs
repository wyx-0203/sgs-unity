using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore
{
    public static class TimerAction
    {
        /// <summary>
        /// 选择x张手牌
        /// </summary>
        public static async Task<List<Card>> SelectHandCard(Player player, int count, string hint, Func<PlayDecision> defaultAI = null)
        {
            // Timer.Instance.isValidCard = card => card.isHandCard;
            // Timer.Instance.refusable = false;
            // Timer.Instance.defaultAI = defaultAI != null ? defaultAI : () => new Decision { action = true, cards = AI.GetRandomCard() };

            return (await new PlayQuery
            {
                player = player,
                hint = hint,
                isValidCard = card => card.isHandCard,
                refusable = false,
                defaultAI = defaultAI != null ? defaultAI : player.game.ai.TryAction
            }.Run(count, 0)).cards;
        }


        /// <summary>
        /// 弃x张手牌
        /// </summary>
        public static async Task DiscardFromHand(Player player, int count)
        {
            string hint = $"请弃置{count}张手牌。";
            await new Discard(player, await SelectHandCard(player, count, hint)).Execute();
        }

        /// <summary>
        /// 展示一张手牌
        /// </summary>
        public static async Task<List<Card>> ShowOneCard(Player player, string hint, int count = 1)
        {
            var cards = await SelectHandCard(player, count, hint);
            var showCard = new ShowCard(player, cards);
            await showCard.Execute();
            return showCard.Cards;
        }

        /// <summary>
        /// 选择其他角色的一张牌
        /// </summary>
        // public static async Task<List<Card>> SelectCardFromElse(Player player, Player dest,string title,string hint, bool judgeArea = false)
        // {
        //     var cards = dest.cards.ToList();
        //     if (judgeArea) cards.AddRange(dest.JudgeCards);

        //     return (await CardPanelRequest.Instance.Run(player, dest, cards)).cards;
        // }

        public static async Task<Card[]> Compete(Player src, Player dest)
        {
            // string hint = ;
            // Timer.Instance.defaultAI = () =>
            // {
            //     var card = src.handCards.OrderBy(x => src.team == dest.team ? x.weight : -x.weight).First();
            //     return new Decision { action = true, cards = new List<Card> { card } };
            // };
            var srcCard = await TimerAction.SelectHandCard(src, 1, $"对{dest}拼点，请选择一张手牌。", () =>
            {
                var card = src.handCards.OrderBy(x => src.team == dest.team ? x.weight : -x.weight).First();
                return new PlayDecision { action = true, cards = new List<Card> { card } };
            });

            // hint = ;
            // Timer.Instance.defaultAI = 
            var destCard = await TimerAction.SelectHandCard(dest, 1, $"{src}对你拼点，请选择一张手牌。", () =>
            {
                var card = dest.handCards.OrderBy(x => src.team == dest.team ? x.weight : -x.weight).First();
                return new PlayDecision { action = true, cards = new List<Card> { card } };
            });

            src.game.cardPile.AddToDiscard(srcCard, src);
            src.game.cardPile.AddToDiscard(destCard, dest);
            await new LoseCard(src, srcCard).Execute();
            await new LoseCard(dest, destCard).Execute();

            return new Card[] { srcCard[0], destCard[0] };
        }

        // public static async Task<Decision> MultiConvert(Player player, List<Card> cards, Predicate<Card> isValidCard = null)
        // {
        //     Timer.Instance.maxCard = 1;
        //     Timer.Instance.minCard = 1;
        //     Timer.Instance.multiConvert.AddRange(cards);
        //     Timer.Instance.isValidCard = isValidCard is null ? card => cards.Contains(card) : card => cards.Contains(card) && isValidCard(card);

        //     var decision = await Timer.Instance.Run(player);
        //     decision.cards.Add(cards.Find(x => x.name == decision.other));
        //     return decision;
        // }
    }
}
