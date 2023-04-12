using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 好施 : Triggered
    {
        public 好施(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.WhenGetCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.WhenGetCard.RemoveEvent(Src);
        }

        public async Task Execute(GetCardFromPile getCard)
        {
            if (!getCard.InGetCardPhase || !await base.ShowTimer()) return;

            Execute();
            (getCard as GetCardFromPile).Count += 2;
            Src.events.FinishPhase[Phase.Get].AddEvent(Src, Give);
            TurnSystem.Instance.AfterTurn += Reset;
        }

        public async Task Give()
        {
            if (Src.HandCardCount <= 5) return;

            int count = Src.HandCardCount / 2;
            int min = SgsMain.Instance.MinHand(Src);

            Timer.Instance.Hint = "请选择" + count + "张手牌，交给一名手牌最少的角色";
            Timer.Instance.IsValidDest = dest => dest.HandCardCount == min;
            Timer.Instance.IsValidCard = card => Src.HandCards.Contains(card);
            Timer.Instance.Refusable = false;
            bool result = await Timer.Instance.Run(Src, count, 1);

            if (isAI)
            {
                result = true;
                var i = SgsMain.Instance.AlivePlayers.Where(x => x.HandCardCount == min && x != Src).ToList();
                i.Sort((x, y) => x.team == Src.team ? -1 : 1);
                Operation.Instance.Dests.Add(i[0]);
                Operation.Instance.Cards.AddRange(Src.HandCards.Take(count));
                Operation.Instance.AICommit();
            }

            var cards = result ? Timer.Instance.Cards : Src.HandCards.Take(count).ToList();
            var dest = result ? Timer.Instance.Dests[0] :
                SgsMain.Instance.AlivePlayers.Find(x => x.HandCardCount == min && x != Src);

            await new GetCardFromElse(dest, Src, cards).Execute();
        }

        protected override void Reset()
        {
            // base.Reset();
            Src.events.FinishPhase[Phase.Get].RemoveEvent(Src);
            TurnSystem.Instance.AfterTurn -= Reset;
        }

        protected override bool AIResult() => Src.HandCardCount < 2
            || SgsMain.Instance.MinHand(Src) == Src.teammate.HandCardCount;
    }

    public class 缔盟 : Active
    {
        public 缔盟(Player src) : base(src) { }

        public override int MaxDest => 2;
        public override int MinDest => 2;
        public override bool IsValidDest(Player dest)
        {
            if (Src == dest) return false;
            return firstDest is null || Mathf.Abs(firstDest.HandCardCount - dest.HandCardCount) <= Src.CardCount;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            // 弃牌
            int count = Mathf.Abs(dests[0].HandCardCount - dests[1].HandCardCount);
            if (count > 0)
            {
                Timer.Instance.Refusable = false;
                bool result = await Timer.Instance.Run(Src, count, 0);
                List<Card> discard = null;
                if (result) discard = Timer.Instance.Cards;
                else if (Src.HandCardCount >= count) discard = Src.HandCards.Take(count).ToList();
                else
                {
                    discard = new List<Card>(Src.HandCards);
                    foreach (var i in Src.Equipages.Values)
                    {
                        if (discard.Count == count) break;
                        if (i != null) discard.Add(i);
                    }
                }
                await new Discard(Src, discard).Execute();
            }

            TurnSystem.Instance.SortDest(dests);
            await base.Execute(dests, cards, other);

            // List<Card> card0 = new List<Card>(dests[0].HandCards);
            // List<Card> card1 = new List<Card>(dests[1].HandCards);
            // await new LoseCard(dests[0], card0).Execute();
            // await new LoseCard(dests[1], card1).Execute();
            // await new GetCard(dests[0], card1).Execute();
            // await new GetCard(dests[1], card0).Execute();
            await new ExChange(dests[0], dests[1]).Execute();
        }
    }

    public class ExChange : PlayerAction<ExChange>
    {
        public ExChange(Player player, Player dest) : base(player)
        {
            Dest = dest;
        }

        public Player Dest { get; private set; }

        public async Task Execute()
        {
            actionView(this);
            List<Card> card0 = new List<Card>(player.HandCards);
            List<Card> card1 = new List<Card>(Dest.HandCards);
            await new LoseCard(player, card0).Execute();
            await new LoseCard(Dest, card1).Execute();
            await new GetCard(player, card1).Execute();
            await new GetCard(Dest, card0).Execute();
        }
    }
}