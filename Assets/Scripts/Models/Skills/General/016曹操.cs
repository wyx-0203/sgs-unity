using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 奸雄 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
        }

        public async Task Execute(Damaged damaged)
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            if (damaged.SrcCard != null)
            {
                var cards = damaged.SrcCard.InDiscardPile();
                await new GetDisCard(Src, cards).Execute();
            }

            await new GetCardFromPile(Src, 1).Execute();
        }
    }
}
