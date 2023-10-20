using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 武圣 : Converted
    {
        public override Card Convert(List<Card> cards) => Card.Convert<杀>(cards);

        public override bool IsValidCard(Card card) => card.isRed && base.IsValidCard(card);

        public override void OnEnable()
        {
            Src.unlimitDst += UnlimitDst;
        }

        public override void OnDisable()
        {
            Src.unlimitDst -= UnlimitDst;
        }

        private bool UnlimitDst(Card card, Player dest) => card is 杀 && card.suit == "方片" && card.IsConvert;
    }

    /// <summary>
    /// 义绝
    /// </summary>
    public class 义绝 : Active
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override bool IsValidDest(Player dest) => dest.HandCardCount > 0 && dest != Src;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);
            dest = decision.dests[0];

            // 弃一张手牌
            await new Discard(Src, decision.cards).Execute();

            // 展示手牌
            Timer.Instance.hint = Src.posStr + "号位对你发动义绝，请展示一张手牌。";
            var showCard = await TimerAction.ShowOneCard(dest);

            // 红色
            if (showCard[0].isRed)
            {
                // 获得牌
                await new GetCardFromElse(Src, dest, showCard).Execute();
                // 回复体力
                if (dest.Hp < dest.HpLimit)
                {
                    Timer.Instance.hint = "是否让" + dest.posStr + "号位回复一点体力？";
                    Timer.Instance.DefaultAI = () => new Decision { action = (dest.team == Src.team) == AI.CertainValue };
                    if ((await Timer.Instance.Run(Src)).action) await new Recover(dest).Execute();
                }
            }
            // 黑色
            else
            {
                isBlack = true;
                dest.disabledCard += DisabledCard;
                foreach (var i in dest.skills.Where(x => !x.isObey)) i.SetActive(false);
                dest.events.WhenDamaged.AddEvent(Src, WhenDamaged);
            }
        }

        private bool isBlack;
        private bool isDamaged;
        private Player dest;

        public bool DisabledCard(Card card) => true;

        public async Task WhenDamaged(Damaged damaged)
        {
            await Task.Yield();
            if (!isDamaged && damaged.Src == Src && damaged.SrcCard is 杀 && damaged.SrcCard.suit == "红桃")
            {
                damaged.Value--;
                isDamaged = true;
            }
        }

        protected override void ResetAfterTurn()
        {
            if (!isBlack) return;
            isBlack = false;

            isDamaged = false;
            dest.disabledCard -= DisabledCard;
            foreach (var i in dest.skills) if (!i.isObey && i.Enabled < 1) i.SetActive(true);
            dest.events.WhenDamaged.RemoveEvent(Src);
        }

        public override Decision AIDecision()
        {
            Timer.Instance.temp.cards = AI.GetRandomCard();
            Timer.Instance.temp.dests = AI.GetValidDest();
            return base.AIDecision();
        }
    }
}
