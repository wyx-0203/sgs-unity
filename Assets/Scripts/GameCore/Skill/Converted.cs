using System.Collections.Generic;

namespace GameCore
{
    public abstract class Converted : Skill
    {
        public abstract Card Convert(List<Card> cards);

        public virtual Card Use(List<Card> cards)
        {
            Execute();
            return Convert(cards);
        }

        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override bool IsValidCard(Card card) => card.useable;

        // public override int MaxDest => Timer.Instance.startPlay.maxDest(Convert(new List<Card> { card }));
        // public override int MinDest => Timer.Instance.startPlay.minDest(Convert(new List<Card> { card }));
        // public override bool IsValidDest(Player dest) => Timer.Instance.startPlay.isValidDest(dest, Convert(new List<Card> { card }));

        // public override bool IsValid => base.IsValid && Timer.Instance.playRequest.isValidCard(Convert(null));

        public override PlayQuery ToPlayQuery(PlayQuery origin)
        {
            // var card=Convert(new List<Card> { card });
            var playInfo = base.ToPlayQuery(origin);
            if (!origin.diffDest)
            {
                playInfo.maxDest = origin.maxDest;
                playInfo.minDest = origin.minDest;
                playInfo.isValidDest = origin.isValidDest;
            }
            else if (MaxCard != 1 || MinCard != 1)
            {
                playInfo.maxDest = origin.maxDestForCard(Convert(null));
                playInfo.minDest = origin.minDestForCard(Convert(null));
                playInfo.isValidDest = player => origin.isValidDestForCard(player, Convert(null));
            }
            else
            {
                playInfo.maxDestForCard = x => origin.maxDestForCard(Convert(new List<Card> { x }));
                playInfo.minDestForCard = x => origin.minDestForCard(Convert(new List<Card> { x }));
                playInfo.isValidDestForCard = (player, card) => origin.isValidDestForCard(player, Convert(new List<Card> { card }));
            }
            return playInfo;
        }
    }
}
