using System;
using System.Collections.Generic;
using System.Linq;
// using UnityEngine;

namespace GameCore
{
    public class AI
    {
        public AI(Game game)
        {
            this.game = game;
        }
        private Game game;

        private List<string> haveDamageSkill = new List<string> { "曹操", "法正", "夏侯惇", "荀彧" };

        private Player player => playQuery.player;
        public PlayQuery playQuery { get; set; }

        public List<Player> GetValidDest()
        {
            if (playQuery.minDest == 0) return new();
            var dests = game.AlivePlayers.Where(x => playQuery.isValidDest(x)).OrderBy(GetDefensePower);
            return dests.Take(playQuery.minDest).ToList();
        }

        public List<Player> GetValidDestForCard(Card card)
        {
            // if (playQuery.minDest == 0) return new();
            var dests = game.AlivePlayers.Where(x => playQuery.isValidDestForCard(x, card));
            switch (card)
            {
                case 铁索连环:
                    dests = dests.Where(x => x.team != playQuery.player.team ^ x.locked).Shuffle();
                    break;
                default:
                    dests = dests.Where(x => x.team != playQuery.player.team).OrderBy(GetDefensePower);
                    break;
            }
            return dests.Take(playQuery.maxDestForCard(card)).ToList();
        }

        // public List<Player> GetValidDestForSkill(Skill skill)
        // {
        //     var playQuery = this.playQuery.skillQuerys.First(x => x.skill == skill.name);
        //     if (playQuery.minDest == 0) return new();
        //     var dests = game.AlivePlayers.Where(x => playQuery.isValidDest(x)).OrderBy(GetDefensePower);
        //     return dests.Take(playQuery.minDest).ToList();
        // }

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
            int maxCard = playQuery.maxCard;
            int minCard = playQuery.minCard;
            // int count=new Random

            // return player.cards.Where(x=>playQuery.isValidCard(x))

            var cards = player.cards.Where(x => playQuery.isValidCard(x));
            if (cards.Count() < minCard) return cards.ToList();

            int count = Math.Min(cards.Count(), maxCard);
            if (count > minCard) count = new Random().Next(minCard, count + 1);

            return cards.Shuffle(count);
        }

        public IEnumerable<Player> GetDestByTeam(Model.Team team) =>
            // var card = Timer.Instance.temp.cards.FirstOrDefault();
            // var isValidDest = Timer.Instance.startPlay.isValidDest;
            game.AlivePlayers.Where(x => playQuery.isValidDest(x) && x.team == team).Shuffle();
        // game.AlivePlayers.Where(x=>x.team==team&&isValidDest(x,))


        // public static IEnumerable<Player> GetAllDests()
        // {
        //     return game.AlivePlayers.Where(x => Timer.Instance.isValidDest(x));
        // }

        public PlayDecision TryAction()
        {
            if (!CertainValue) return new();

            var cards = GetRandomCard();
            var dests = GetValidDest();
            // Debug.Log($"card.count: {cards.Count}");

            if (cards.Count < playQuery.minCard || dests.Count < playQuery.minDest) return new();
            else return new PlayDecision { action = true, cards = cards, dests = dests };
        }

        private const double certainX = 1d;
        public static bool CertainValue => new Random().NextDouble() < certainX;

        // public static List<T> Shuffle<T>(List<T> list, int count = 1)
        // {
        //     // 随机取一个元素与第i个元素交换
        //     for (int i = 0; i < count; i++)
        //     {
        //         int t = Random.Range(i, list.Count);
        //         var item = list[i];
        //         list[i] = list[t];
        //         list[t] = item;
        //     }
        //     return list.GetRange(0, count);
        // }
    }
}
