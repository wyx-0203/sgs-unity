using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{

    public class 当先 : Triggered
    {
        public override bool isObey => true;

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Prepare].AddEvent(Src, Execute);
            Src.events.StartPhase[Phase.Play].AddEvent(Src, StartPerform);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Prepare].RemoveEvent(Src);
            Src.events.StartPhase[Phase.Play].RemoveEvent(Src);
        }

        public async Task Execute()
        {
            await base.Execute();
            TurnSystem.Instance.ExtraPhase.Add(Phase.Play);
            inSkill = true;
        }

        private bool inSkill;
        private bool fuliHasInvoked;

        public async Task StartPerform()
        {
            if (!inSkill) return;
            inSkill = false;

            if (fuliHasInvoked)
            {
                Timer.Instance.hint = "是否失去1点体力并从弃牌堆获得一张【杀】？";
                Timer.Instance.AIDecision = () => new Decision { action = Src.Hp > 1 && Src.FindCard<杀>() is null };
                if (!(await Timer.Instance.Run(Src)).action) return;
            }

            await new UpdateHp(Src, -1).Execute();
            var card = CardPile.Instance.DiscardPile.Find(x => x is 杀);
            if (card != null) await new GetDisCard(Src, new List<Card> { card }).Execute();
        }
    }
}