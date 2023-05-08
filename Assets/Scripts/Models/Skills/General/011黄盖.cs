using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 苦肉 : Active
    {
        public 苦肉(Player src) : base(src) { }

        public override int MaxCard => 1;
        public override int MinCard => 1;

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            await base.Execute(dests, cards, other);

            await new Discard(Src, cards).Execute();
            await new UpdateHp(Src, -1).Execute();
        }
    }

    public class 诈降 : Triggered
    {
        public 诈降(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            Src.events.AfterLoseHp.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterLoseHp.RemoveEvent(Src);
        }

        public async Task Execute(UpdateHp updataHp)
        {
            Execute();

            await new GetCardFromPile(Src, -3 * updataHp.Value).Execute();

            if (TurnSystem.Instance.CurrentPlayer != Src || TurnSystem.Instance.CurrentPhase != Phase.Perform)
            {
                return;
            }

            // 出杀次数加1
            Src.杀Count--;
            // 红杀无距离限制
            Src.unlimitedDst += IsUnlimited;
            // 红杀不可闪避
            Src.events.AfterUseCard.AddEvent(Src, WhenUseSha);

            TurnSystem.Instance.AfterTurn += ResetEffect;
        }

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && (card.Suit == "红桃" || card.Suit == "方片");

        private async Task WhenUseSha(Card card)
        {
            if (card is 杀 && (card.Suit == "红桃" || card.Suit == "方片" || card.Suit == "红色"))
            {
                await Task.Yield();
                foreach (var i in card.Dests) (card as 杀).ShanCount[i.position] = 0;
            }
        }

        private void ResetEffect()
        {
            Src.unlimitedDst -= IsUnlimited;
            Src.events.AfterUseCard.RemoveEvent(Src);
            TurnSystem.Instance.AfterTurn -= ResetEffect;
        }
    }
}