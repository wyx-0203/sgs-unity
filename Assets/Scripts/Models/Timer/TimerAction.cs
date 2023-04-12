using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public static class TimerAction
    {
        /// <summary>
        /// 选择手牌
        /// </summary>
        public static async Task<List<Card>> SelectHandCard(Player player, int count)
        {
            Timer.Instance.IsValidCard = (card) => player.HandCards.Contains(card);
            Timer.Instance.Refusable = false;
            bool result = await Timer.Instance.Run(player, count, 0);
            return result ? Timer.Instance.Cards : player.HandCards.Take(count).ToList();
        }


        /// <summary>
        /// 弃手牌
        /// </summary>
        public static async Task DiscardFromHand(Player player, int count)
        {
            Timer.Instance.Hint = "请弃置" + count.ToString() + "张手牌。";
            await new Discard(player, await SelectHandCard(player, count)).Execute();
        }

        /// <summary>
        /// 展示手牌
        /// </summary>
        public static async Task<List<Card>> ShowCardTimer(Player player, int count = 1)
        {
            var cards = await SelectHandCard(player, count);
            var showCard = new ShowCard(player, cards);
            await showCard.Execute();
            return showCard.Cards;
        }

        // public static async Task Compete(Player src,Player dest)
        // {
        //     // 
        // }
    }
}
