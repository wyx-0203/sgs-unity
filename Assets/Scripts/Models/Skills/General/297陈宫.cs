using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 明策 : Active
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidCard(Card card) => !card.IsConvert && (card is 杀 || card is Equipment);
        public override bool IsValidDest(Player dest) => firstDest is null ? dest != Src : firstDest.DestInAttackRange(dest);

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);
            var dest = decision.dests[0];

            await new GetCardFromElse(dest, Src, decision.cards).Execute();

            Timer.Instance.hint = "视为对该角色使用一张杀，或摸一张牌";
            Timer.Instance.isValidDest = dest => dest == decision.dests[1];

            // 若阵营不同，则选择出杀
            Timer.Instance.AIDecision = dest.team != decision.dests[1].team || !AI.CertainValue ? AI.AutoDecision : () => new();

            decision = await Timer.Instance.Run(dest, 0, 1);
            if (decision.action) await Card.Convert<杀>().UseCard(dest, decision.dests);
            else await new GetCardFromPile(dest, 1).Execute();
        }

        public override Decision AIDecision()
        {
            var cards = AI.GetRandomCard();
            var dests = AI.GetDestByTeam(Src.team).ToList();
            if (cards.Count == 0 || dests.Count == 0) return new();

            Timer.Instance.temp.cards = cards;
            Timer.Instance.temp.dests.Add(dests[0]);

            var dest1 = AI.GetValidDest().Find(x => x != dests[0]);
            if (dest1 != null) Timer.Instance.temp.dests.Add(dest1);
            return base.AIDecision();
        }
    }

    public class 智迟 : Triggered
    {
        public override bool isObey => true;

        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
            Src.disableForMe += Disable;
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
            Src.disableForMe -= Disable;
        }

        private bool invoked;

        public async Task Execute(Damaged damaged)
        {
            if (TurnSystem.Instance.CurrentPlayer == Src) return;

            await Execute();
            invoked = true;
        }

        public bool Disable(Card card) => invoked && (card is 杀 || card.type == "锦囊牌");

        protected override void ResetAfterTurn()
        {
            invoked = false;
        }
    }
}