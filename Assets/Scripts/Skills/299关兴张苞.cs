using GameCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phase = Model.Phase;

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
        public override bool passive => true;
        private Card card => src.FindSkill<_Converted>().card;
        private bool invoked;

        protected override bool OnMakeDamage(Damage damaged) => damaged.SrcCard == card
            && game.turnSystem.CurrentPlayer == src
            && game.turnSystem.CurrentPhase == Phase.Play
            && !invoked;

        protected override async Task Invoke(PlayDecision decision)
        {
            Execute();
            invoked = true;
            await new AddSkill(src, new List<string> { "武圣", "咆哮" }).Execute();
            game.turnSystem.AfterTurn += async () =>
            {
                invoked = false;
                await new RemoveSkill(src, new List<string> { "武圣", "咆哮" }).Execute();
            };
        }
    }
}


