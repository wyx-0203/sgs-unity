using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Model
{
    public class 龙吟 : Triggered
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;

        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers) i.events.WhenUseCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers) i.events.WhenUseCard.RemoveEvent(Src);
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀 || card.Src != TurnSystem.Instance.CurrentPlayer) return;
            dest = card.Src;
            sha = card;
            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            await new Discard(Src, decision.cards).Execute();
            dest.杀Count--;
            if (card.isRed) await new GetCardFromPile(Src, 1).Execute();
            if (decision.cards[0].weight == card.weight) (Src.skills.Find(x => x is 竭忠) as 竭忠).IsDone = false;
        }

        private Player dest;
        private Card sha;

        public override Decision AIDecision()
        {
            if (Src.CardCount == 0 || dest.team != Src.team && (!sha.isRed || UnityEngine.Random.value < 0.7f)) return new();
            return AI.TryAction();
        }
    }

    public class 竭忠 : Triggered, Ultimate
    {
        // public 竭忠(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Play].AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Play].RemoveEvent(Src);
        }

        public bool IsDone { get; set; } = false;

        public async Task Execute()
        {
            int count = Src.HpLimit - Src.HandCardCount;
            if (count < 1 || !(await base.WaitDecision()).action) return;
            await base.Execute();

            IsDone = true;
            await new GetCardFromPile(Src, count).Execute();
        }

        public override Decision AIDecision() => new Decision { action = Src.HpLimit - Src.HandCardCount > 1 };
    }
}