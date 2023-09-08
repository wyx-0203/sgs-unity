using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 制衡 : Active
    {
        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);

            int count = decision.cards.Count;
            if (Src.HandCardCount > 0)
            {
                count++;
                foreach (var i in Src.HandCards)
                {
                    if (!decision.cards.Contains(i))
                    {
                        count--;
                        break;
                    }
                }
            }

            await new Discard(Src, decision.cards).Execute();
            await new GetCardFromPile(Src, count).Execute();
        }

        public override Decision AIDecision()
        {
            // 优先出牌
            if (TurnSystem.Instance.PlayDecisions.Count > 0 && AI.CertainValue) return new();

            Timer.Instance.temp.cards = AI.GetRandomCard();
            return base.AIDecision();
        }
    }
}