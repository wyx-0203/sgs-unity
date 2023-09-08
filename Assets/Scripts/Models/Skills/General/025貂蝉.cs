using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 离间 : Active
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidDest(Player dest) => dest != Src && dest.general.gender;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);

            await new Discard(Src, decision.cards).Execute();
            await Card.Convert<决斗>().UseCard(decision.dests[1], new List<Player> { decision.dests[0] });
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetValidDest();
            if (dests.Count < 2 || dests[0].team == Src.team) return new();

            Timer.Instance.temp.cards = AI.GetRandomCard();
            Timer.Instance.temp.dests = dests.GetRange(0, 2);
            return base.AIDecision();
        }
    }

    public class 闭月 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.End].AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
        }

        public async Task Execute()
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            await new GetCardFromPile(Src, Src.HandCardCount == 0 ? 2 : 1).Execute();
        }
    }
}