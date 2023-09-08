using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 好施 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.WhenGetCard.AddEvent(Src, Execute);
            Src.events.FinishPhase[Phase.Get].AddEvent(Src, Give);
        }

        public override void OnDisable()
        {
            Src.events.WhenGetCard.RemoveEvent(Src);
            Src.events.FinishPhase[Phase.Get].AddEvent(Src, Give);
        }

        public async Task Execute(GetCard getCard)
        {
            if (getCard is not GetCardFromPile getCardFromPile) return;
            if (!getCardFromPile.InGetCardPhase) return;

            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            getCardFromPile.Count += 2;
            invoked = true;
        }

        private bool invoked;

        public async Task Give()
        {
            if (!invoked || Src.HandCardCount <= 5) return;

            int count = Src.HandCardCount / 2;
            int min = SgsMain.Instance.MinHandCard(Src);

            Timer.Instance.hint = "请选择" + count + "张手牌，交给一名手牌最少的角色";
            Timer.Instance.isValidDest = dest => dest.HandCardCount == min;
            Timer.Instance.isValidCard = card => Src.HandCards.Contains(card);
            Timer.Instance.refusable = false;
            Timer.Instance.AIDecision = () => new Decision
            {
                action = true,
                cards = AI.GetRandomCard(),
                dests = AI.GetAllDests().OrderBy(x => x.team == Src.team ? -1 : 1).Take(1).ToList(),
            };

            var decision = await Timer.Instance.Run(Src, count, 1);
            await new GetCardFromElse(decision.dests[0], Src, decision.cards).Execute();
        }

        protected override void ResetAfterTurn()
        {
            invoked = false;
        }
    }

    public class 缔盟 : Active
    {
        public override int MaxDest => 2;
        public override int MinDest => 2;
        public override bool IsValidDest(Player dest)
        {
            if (Src == dest) return false;
            return firstDest is null || Mathf.Abs(firstDest.HandCardCount - dest.HandCardCount) <= Src.CardCount;
        }

        public override async Task Execute(Decision decision)
        {
            TurnSystem.Instance.SortDest(decision.dests);
            await base.Execute(decision);

            // 弃牌
            int count = Mathf.Abs(decision.dests[0].HandCardCount - decision.dests[1].HandCardCount);
            if (count > 0)
            {
                Timer.Instance.hint = "请弃置" + count + "张牌";
                Timer.Instance.refusable = false;
                Timer.Instance.AIDecision = AI.AutoDecision;
                var _decision = await Timer.Instance.Run(Src, count, 0);
                await new Discard(Src, _decision.cards).Execute();
            }

            // 交换手牌
            await new ExChange(decision.dests[0], decision.dests[1]).Execute();
        }
        public override Decision AIDecision()
        {
            // 将队友按手牌数量生序排列，敌人降序排列
            var teammates = SgsMain.Instance.AlivePlayers.Where(x => x.team == Src.team && x != Src).OrderBy(x => x.HandCardCount).ToArray();
            var dests = SgsMain.Instance.AlivePlayers.Where(x => x.team != Src.team).OrderBy(x => -x.HandCardCount).ToArray();

            if (teammates.Count() == 0) return new();
            int i = 0, j = 0, diff = dests[j].HandCardCount - teammates[i].HandCardCount;

            // 找到手牌相差最大的情况
            while (diff > Src.CardCount && (i < teammates.Length - 1 || j < dests.Length - 1))
            {
                if (i == teammates.Length - 1) j++;
                else if (j == dests.Length - 1) i++;
                else if (teammates[i + 1].HandCardCount - teammates[i].HandCardCount < dests[j].HandCardCount - dests[j + 1].HandCardCount) i++;
                else j++;
                diff = dests[j].HandCardCount - teammates[i].HandCardCount;
            }

            if (diff < 0 || diff > Src.HandCardCount) return new();
            Timer.Instance.temp.dests.Add(teammates[i]);
            Timer.Instance.temp.dests.Add(dests[j]);
            return base.AIDecision();
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