using System.Threading.Tasks;

namespace Model
{
    public class 苦肉 : Active
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);

            await new Discard(Src, decision.cards).Execute();
            await new UpdateHp(Src, -1).Execute();
        }

        public override Decision AIDecision()
        {
            if (Src.Hp < 2) return new();
            Timer.Instance.temp.cards = AI.GetRandomCard();
            return base.AIDecision();
        }
    }

    public class 诈降 : Triggered
    {
        public override bool isObey => true;

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
            await Execute();

            await new GetCardFromPile(Src, -3 * updataHp.Value).Execute();

            if (TurnSystem.Instance.CurrentPlayer != Src || TurnSystem.Instance.CurrentPhase != Phase.Play) return;

            // 出杀次数加1
            Src.杀Count--;
            // 红杀无距离限制
            Src.unlimitDst += IsUnlimited;
            // 红杀不可闪避
            Src.events.AfterUseCard.AddEvent(Src, WhenUseSha);
        }

        private bool invoked;

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && card.isRed;

        private async Task WhenUseSha(Card card)
        {
            if (card is 杀 sha && card.isRed)
            {
                await Task.Yield();
                foreach (var i in card.Dests) sha.ShanCount[i.position] = 0;
            }
        }

        protected override void ResetAfterPlay()
        {
            base.ResetAfterPlay();
            Src.unlimitDst -= IsUnlimited;
            Src.events.AfterUseCard.RemoveEvent(Src);
        }
    }
}