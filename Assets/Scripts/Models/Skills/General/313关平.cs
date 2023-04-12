using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Model
{
    public class 龙吟 : Triggered
    {
        public 龙吟(Player src) : base(src) { }

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
            if (!await base.ShowTimer()) return;
            Execute();

            var card1 = Timer.Instance.Cards[0];
            await new Discard(Src, Timer.Instance.Cards).Execute();
            dest.ShaCount--;
            if (card.Suit == "红桃" || card.Suit == "方片") await new GetCardFromPile(Src, 1).Execute();
            if (card1.Weight == card.Weight) (Src.skills.Find(x => x is 竭忠) as 竭忠).IsDone = false;
            // await new GetCardFromPile(getCardFromElse.Dest, 1).Execute();
        }

        private Player dest;

        protected override bool AIResult() => false;
    }

    public class 竭忠 : Triggered, Ultimate
    {
        public 竭忠(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Perform].AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Perform].RemoveEvent(Src);
        }

        public bool IsDone { get; set; } = false;

        public new async Task Execute()
        {
            int count = Src.HpLimit - Src.HandCardCount;
            if (count < 1 || !await base.ShowTimer()) return;
            base.Execute();
            IsDone = true;
            await new GetCardFromPile(Src, count).Execute();
        }

        protected override bool AIResult() => Src.HpLimit - Src.HandCardCount > 1;
    }
}