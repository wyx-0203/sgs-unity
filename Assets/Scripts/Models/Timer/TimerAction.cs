using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public static class TimerAction
    {
        /// <summary>
        /// 选择x张手牌
        /// </summary>
        public static async Task<List<Card>> SelectHandCard(Player player, int count)
        {
            Timer.Instance.isValidCard = card => player.HandCards.Contains(card);
            Timer.Instance.refusable = false;
            Timer.Instance.AIDecision = () => new Decision { action = true, cards = AI.GetRandomCard() };

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



        public static async Task<List<Card>> SelectCard(Player player, Player dest, bool judgeArea = false)
        {
            var cards = dest.HandCards.Union(dest.Equipments.Values).Where(x => x != null);
            if (judgeArea) cards = cards.Union(dest.JudgeArea);

            return (await CardPanel.Instance.Run(player, dest, cards.ToList())).cards;
        }

        // public static async Task Compete(Player src,Player dest)
        // {
        //     // 
        // }
    }
}
