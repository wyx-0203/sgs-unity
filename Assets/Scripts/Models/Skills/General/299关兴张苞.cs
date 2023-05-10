using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 父魂 : Converted
    {
        public 父魂(Player src) : base(src) { }
        public override string CardName => "杀";

        public override int MaxCard => 2;
        public override int MinCard => 2;

        public override Card Execute(List<Card> cards) => Card.Convert<杀>(cards);

        public override void Execute()
        {
            base.Execute();
            primitives = Timer.Instance.cards[0].PrimiTives;
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
                || TurnSystem.Instance.CurrentPhase != Phase.Perform
                || isDone) return;

            await Task.Yield();
            new UpdateSkill(Src, new List<string> { "武圣", "咆哮" }).Add();
            TurnSystem.Instance.AfterTurn += Reset;
            isDone = true;
        }

        protected override void Reset()
        {
            new UpdateSkill(Src, new List<string> { "武圣", "咆哮" }).Remove();
            isDone = false;
            TurnSystem.Instance.AfterTurn -= Reset;
        }
    }
}