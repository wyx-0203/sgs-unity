using System.Collections.Generic;

namespace Model
{
    public abstract class Converted : Skill
    {
        // public Converted(Player src) : base(src) { }

        // 转化牌名称
        // public virtual string CardName => "";

        public abstract Card Convert(List<Card> cards);

        // public Card Execute(List<Card> cards)
        // {
        //     Execute();
        //     return Convert(cards);
        // }

        public override int MaxCard => 1;

        public override int MinCard => 1;

        public override bool IsValidCard(Card card) => !Src.DisabledCard(card);

        public override bool IsValid => base.IsValid
            && Timer.Instance.maxCard > 0
            && Timer.Instance.isValidCard(Convert(null));

        // public override void Execute()
        // {
        //     // Dests = TimerTask.Instance.Dests;
        //     base.Execute();
        // }
    }
}
