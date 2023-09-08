using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model
{
    public static class AI
    {
        private static List<string> haveDamageSkill = new List<string> { "曹操", "法正", "夏侯惇", "荀彧" };

        private static Player player => Timer.Instance.players[0];

        public static List<Player> GetValidDest()
        {
            if (Timer.Instance.minDest() == 0) return new();
            var dests = SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).OrderBy(GetDefensePower);
            return dests.Take(Timer.Instance.minDest()).ToList();
        }

        public static int GetDefensePower(Player dest)
        {
            int power = dest.Hp * 3 + dest.HandCardCount;
            if (haveDamageSkill.Contains(dest.general.name)) power += (dest.Hp - 1) * 4;
            if (dest.team == player.team) power += 1000;
            return power;
        }

        public static List<Card> GetRandomCard()
        {
            var cards = player.HandCards.Union(player.Equipments.Values).Where(x => x != null && Timer.Instance.isValidCard(x)).ToList();
            if (cards.Count < Timer.Instance.minCard) return cards;

            int count = Mathf.Min(cards.Count, Timer.Instance.maxCard);
            if (count > Timer.Instance.minCard) count = Random.Range(Timer.Instance.minCard, count);

            return GetRandomItem(cards, count);
        }

        public static IEnumerable<Player> GetDestByTeam(bool team)
        {
            return SgsMain.Instance.AlivePlayers.Where(x => x.team == team && Timer.Instance.isValidDest(x));
        }


        public static IEnumerable<Player> GetAllDests()
        {
            return SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x));
        }

        public static Decision AutoDecision()
        {
            if (!CertainValue) return new();

            var cards = GetRandomCard();
            var dests = GetValidDest();

            if (cards.Count < Timer.Instance.minCard || dests.Count < Timer.Instance.minDest()) return new();
            else return new Decision { action = true, cards = cards, dests = dests };
        }

        private const float certainX = 1f;
        public static bool CertainValue => Random.value < certainX;

        public static List<T> GetRandomItem<T>(List<T> list, int count = 1)
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
