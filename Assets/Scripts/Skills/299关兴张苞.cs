using GameCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 父魂 : Skill.Multi
{
    public override List<Skill> skills { get; } = new List<Skill> { new _Converted(), new _Ondamaged() };

    public class _Converted : Converted
    {
        public override int MaxCard => 2;
        public override int MinCard => 2;

        public override Card Convert(List<Card> cards) => Card.Convert<杀>(src, cards);

        public override Card Use(List<Card> cards)
        {
            var card = base.Use(cards);
            this.card = card;
            return card;
        }

        public Card card { get; private set; }

        public override bool IsValidCard(Card card) => card.isHandCard && base.IsValidCard(card);
    }

    public class _Ondamaged : Triggered
    {
        public override bool isObey => true;
        private Card card => src.FindSkill<_Converted>().card;
        private bool invoked;

        protected override bool OnMakeDamage(Damaged damaged) => damaged.SrcCard == card
            && TurnSystem.Instance.CurrentPlayer == src
            && TurnSystem.Instance.CurrentPhase == Phase.Play
            && !invoked;

        protected override async Task Invoke(Decision decision)
        {
            Execute();
            invoked = true;
            await new UpdateSkill(src, new List<string> { "武圣", "咆哮" }).Add();
            TurnSystem.Instance.AfterTurn += () =>
            {
                invoked = false;
                new UpdateSkill(src, new List<string> { "武圣", "咆哮" }).Remove();
            };
        }
    }
}


