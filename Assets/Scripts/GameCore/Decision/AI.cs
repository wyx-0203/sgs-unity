using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public class AI : Singleton<AI>
    {
        private List<string> haveDamageSkill = new List<string> { "曹操", "法正", "夏侯惇", "荀彧" };

        private Player player => playRequest.player;
        public PlayQuery playRequest { get; set; }

        public List<Player> GetValidDest()
        {
            if (playRequest.minDest == 0) return new();
            var dests = Game.Instance.AlivePlayers.Where(x => playRequest.isValidDest(x)).OrderBy(GetDefensePower);
            return dests.Take(playRequest.minDest).ToList();
        }

        public int GetDefensePower(Player dest)
        {
            int power = dest.hp * 3 + dest.handCardsCount;
            if (haveDamageSkill.Contains(dest.general.name)) power += (dest.hp - 1) * 4;
            if (dest.team == player.team) power += 1000;
            return power;
        }

        // private static float Evaluate(Card card,Player dest)
        // {
        //     // 是否无效
        //     // 

        //     float sum = 0;
        //     foreach (var i in SgsMain.Instance.AlivePlayers)
        //     {
        //         float value = i.handCardsCount;
        //         value += i.Equipments.Values.Count * 1.5f;
        //         value -= i.JudgeCards.Count * 2;
        //         value += i.hp * 1.6f;
        //         sum += i.team == node.team ? value : -value;
        //     }
        //     Debug.Log("Q=" + sum + "\nnode=" + node);
        //     return sum;
        // }

        public List<Card> GetRandomCard()
        {
            int maxCard = playRequest.maxCard;
            int minCard = playRequest.minCard;

            var cards = player.cards.Where(x => playRequest.isValidCard(x)).ToList();
            if (cards.Count < minCard) return cards;

            int count = Mathf.Min(cards.Count, maxCard);
            if (count > minCard) count = Random.Range(minCard, count);

            return Shuffle(cards, count);
        }

        public IEnumerable<Player> GetDestByTeam(Model.Team team) =>
            // var card = Timer.Instance.temp.cards.FirstOrDefault();
            // var isValidDest = Timer.Instance.startPlay.isValidDest;
            Game.Instance.AlivePlayers.Where(x => playRequest.isValidDest(x)).OrderBy(x => (x.team == team ? -1 : 1) * Random.Range(0, 8));
        // Game.Instance.AlivePlayers.Where(x=>x.team==team&&isValidDest(x,))


        // public static IEnumerable<Player> GetAllDests()
        // {
        //     return Game.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x));
        // }

        public PlayDecision TryAction()
        {
            if (!CertainValue) return new();

            var cards = GetRandomCard();
            var dests = GetValidDest();

            if (cards.Count < playRequest.minCard || dests.Count < playRequest.minDest) return new();
            else return new PlayDecision { action = true, cards = cards, dests = dests };
        }

        private const float certainX = 1f;
        public static bool CertainValue => Random.value < certainX;

        public static List<T> Shuffle<T>(List<T> list, int count = 1)
        {
            // 随机取一个元素与第i个元素交换
            for (int i = 0; i < count; i++)
            {
                int t = Random.Range(i, list.Count);
                var item = list[i];
                list[i] = list[t];
                list[t] = item;
            }
            return list.GetRange(0, count);
        }
    }
}
