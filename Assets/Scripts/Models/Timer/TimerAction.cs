using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class TimerAction
    {
        /// <summary>
        /// 选择x张手牌
        /// </summary>
        public static async Task<List<Card>> SelectHandCard(Player player, int count, Func<Decision> defaultAI = null)
        {
            Timer.Instance.isValidCard = card => card.isHandCard;
            Timer.Instance.refusable = false;
            Timer.Instance.DefaultAI = defaultAI != null ? defaultAI : () => new Decision { action = true, cards = AI.GetRandomCard() };

            return (await Timer.Instance.Run(player, count, 0)).cards;
        }


        /// <summary>
        /// 弃x张手牌
        /// </summary>
        public static async Task DiscardFromHand(Player player, int count)
        {
            Timer.Instance.hint = "请弃置" + count.ToString() + "张手牌。";
            await new Discard(player, await SelectHandCard(player, count)).Execute();
        }

        /// <summary>
        /// 展示一张手牌
        /// </summary>
        public static async Task<List<Card>> ShowOneCard(Player player, int count = 1)
        {
            var cards = await SelectHandCard(player, count);
            var showCard = new ShowCard(player, cards);
            await showCard.Execute();
            return showCard.Cards;
        }


        /// <summary>
        /// 选择其他角色的一张牌
        /// </summary>
        public static async Task<List<Card>> SelectOneCard(Player player, Player dest, bool judgeArea = false)
        {
            var cards = dest.cards.ToList();
            if (judgeArea) cards.AddRange(dest.JudgeCards);

            return (await CardPanel.Instance.Run(player, dest, cards)).cards;
        }

        // public static async Task Compete(Player src,Player dest)
        // {
        //     // 
        // }
        public static async Task<Card[]> Compete(Player src, Player dest)
        {
            Timer.Instance.hint = "对" + dest + "拼点，请选择一张手牌。";
            Timer.Instance.DefaultAI = () =>
            {
                var card = src.HandCards.OrderBy(x => src.team == dest.team ? x.weight : -x.weight).First();
                return new Decision { action = true, cards = new List<Card> { card } };
            };
            var srcCard = await TimerAction.SelectHandCard(src, 1);

            Timer.Instance.hint = src + "对你拼点，请选择一张手牌。";
            Timer.Instance.DefaultAI = () =>
            {
                var card = dest.HandCards.OrderBy(x => src.team == dest.team ? x.weight : -x.weight).First();
                return new Decision { action = true, cards = new List<Card> { card } };
            };
            var destCard = await TimerAction.SelectHandCard(dest, 1);

            CardPile.Instance.AddToDiscard(srcCard);
            CardPile.Instance.AddToDiscard(destCard);
            await new LoseCard(src, srcCard).Execute();
            await new LoseCard(dest, destCard).Execute();

            return new Card[] { srcCard[0], destCard[0] };
        }
    }
}
