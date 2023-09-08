using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 享乐 : Triggered
    {
        public override bool isObey => true;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.events.AfterUseCard.AddEvent(Src, Execute);
            }
            Src.disableForMe += DisableForMe;
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.events.AfterUseCard.RemoveEvent(Src);
            }
            Src.disableForMe -= DisableForMe;
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀 || !card.Dests.Contains(Src)) return;
            await Execute();

            Timer.Instance.hint = "请弃置一张基本牌，否则此【杀】对刘禅无效。";
            Timer.Instance.isValidCard = x => x.type == "基本牌" && !x.IsConvert;
            Timer.Instance.AIDecision = () => card.Src.team != Src.team ? AI.AutoDecision() : new(); ;

            var decision = await Timer.Instance.Run(card.Src, 1, 0);
            if (decision.action) await new Discard(card.Src, decision.cards).Execute();
            else disableForMe = true;
        }

        private bool disableForMe;
        public bool DisableForMe(Card card) => card is 杀 && disableForMe;
    }

    public class 放权 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Play].AddEvent(Src, ExecuteInPlay);
            Src.events.StartPhase[Phase.End].AddEvent(Src, ExecuteInEnd);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Play].RemoveEvent(Src);
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
        }

        private bool invoked = false;

        public async Task ExecuteInPlay()
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            TurnSystem.Instance.SkipPhase[Phase.Play] = true;
            invoked = true;
        }

        public async Task ExecuteInEnd()
        {
            if (!invoked || Src.HandCardCount == 0) return;

            Timer.Instance.hint = "弃置一张手牌并令一名其他角色获得一个额外的回合";
            Timer.Instance.isValidCard = x => Src.HandCards.Contains(x);
            Timer.Instance.isValidDest = x => x != Src;
            Timer.Instance.AIDecision = () =>
            {
                var dests = AI.GetDestByTeam(Src.team).Take(1);
                if (dests.Count() == 0 || AI.CertainValue) return new();

                var cards = AI.GetRandomCard();
                return new Decision { action = true, cards = cards, dests = dests.ToList() };
            };

            var decision = await Timer.Instance.Run(Src, 1, 1);
            if (!decision.action) return;
            await Execute(decision);

            await new Discard(Src, decision.cards).Execute();
            TurnSystem.Instance.ExtraTurn = decision.dests[0];
        }

        public override Decision AIDecision()
        {
            if (SgsMain.Instance.AlivePlayers.Where(x => x.team == Src.team).Count() < 2) return new();
            else if (Src.HandCardCount - Src.HandCardLimit > 2 && AI.CertainValue) return new();
            else return AI.AutoDecision();
        }
    }
}
