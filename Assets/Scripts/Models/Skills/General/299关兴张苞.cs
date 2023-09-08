using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 父魂 : Converted
    {
        public override int MaxCard => 2;
        public override int MinCard => 2;

        public override Card Convert(List<Card> cards) => Card.Convert<杀>(cards);

        public override async Task Execute(Decision decision = null)
        {
            primitives = decision.cards[0].PrimiTives;
            await base.Execute(decision);
        }

        private List<Card> primitives;

        public override bool IsValidCard(Card card) => Src.HandCards.Contains(card) && base.IsValidCard(card);

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.AfterDamaged.AddEvent(Src, OnDamaged);
            }
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.AfterDamaged.RemoveEvent(Src);
            }
        }

        private bool isDone = false;

        public async Task OnDamaged(Damaged damaged)
        {
            if (damaged.Src != Src
                || damaged.SrcCard is null
                || damaged.SrcCard.PrimiTives != primitives
                || TurnSystem.Instance.CurrentPlayer != Src
                || TurnSystem.Instance.CurrentPhase != Phase.Play
                || isDone) return;

            await Task.Yield();
            new UpdateSkill(Src, new List<string> { "武圣", "咆哮" }).Add();
            TurnSystem.Instance.AfterTurn += ResetAfterTurn;
            isDone = true;
        }

        protected override void ResetAfterTurn()
        {
            new UpdateSkill(Src, new List<string> { "武圣", "咆哮" }).Remove();
            isDone = false;
            TurnSystem.Instance.AfterTurn -= ResetAfterTurn;
        }
    }
}