using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore
{
    public class PlayDecision
    {
        public bool action = false;
        public List<Card> cards = new();
        public List<Player> dests = new();
        public Skill skill;
        public Card virtualCard;
        public Player src;

        public PlayDecision() { }

        public PlayDecision(Model.PlayDecision model, PlayQuery playerQuery)
        {
            try
            {
                action = model.action;
                if (!action) return;

                var game = playerQuery.player.game;
                cards = model.cards.Select(x => game.cardPile.cards[x]).ToList();
                dests = model.dests.Select(x => game.players[x]).ToList();
                src = game.players[model.player];
                skill = src.FindSkill(model.skill);
                virtualCard = Card.NewVirtualCard(model.virtualCard, src);

                var list = playerQuery.skillQuerys.ToList();
                list.Add(playerQuery);
                // UnityEngine.Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                var pq = list.FirstOrDefault(x => x.skill == model.skill && IsValid(x));
                if (pq is null) action = false;
            }
            catch (Exception) { action = false; }
        }

        public bool IsValid(PlayQuery playQuery)
        {
            if (cards.Count < playQuery.minCard
                || cards.Count > playQuery.maxCard
                || !cards.All(x => playQuery.isValidCard(x))) return false;

            if (!playQuery.diffDest)
            {
                return dests.Count >= playQuery.minDest
                    && dests.Count <= playQuery.maxDest
                    && dests.All(x => playQuery.isValidDest(x));
            }

            else
            {
                var card = virtualCard is null ? cards[0] : virtualCard;
                return dests.Count >= playQuery.minDestForCard(card)
                    && dests.Count <= playQuery.maxDestForCard(card)
                    && dests.All(x => playQuery.isValidDestForCard(x, card));
            }
        }

        public Model.PlayDecision ToMessage() => new Model.PlayDecision
        {
            action = action,
            cards = cards.Select(x => x.id).ToList(),
            dests = dests.Select(x => x.position).ToList(),
            skill = skill?.name,
            virtualCard = virtualCard != null ? virtualCard.id : 0,
            player = src.position,
        };

        public override string ToString()
        {
            string str = "action=" + action;
            if (!action) return str + '\n';

            str += "\ncards=" + string.Join(' ', cards);
            str += "\ndests=" + string.Join(' ', dests);
            str += "\nskill=" + skill?.name;
            str += "\nsrc=" + src;
            return str + '\n';
        }
    }
}