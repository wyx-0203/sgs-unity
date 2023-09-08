using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 精策 : Triggered
    {
        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.End].AddEvent(Src, Execute);
            Src.events.WhenUseCard.AddEvent(Src, WhenUseCard);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
            Src.events.WhenUseCard.RemoveEvent(Src);
        }

        public async Task Execute()
        {
            if (cards.Count < Src.Hp || !(await base.WaitDecision()).action) return;
            if (cards.Select(x => x.suit).Distinct().Count() < Src.Hp)
            {
                Timer.Instance.hint = "点击确定执行一个额外的摸牌阶段，点击取消执行出牌阶段";
                Timer.Instance.AIDecision = () => new Decision { action = UnityEngine.Random.value < 0.5f };
                TurnSystem.Instance.ExtraPhase.Add((await Timer.Instance.Run(Src)).action ? Phase.Get : Phase.Play);
            }
            else
            {
                TurnSystem.Instance.ExtraPhase.Add(Phase.Get);
                TurnSystem.Instance.ExtraPhase.Add(Phase.Play);
            }
            await base.Execute();
        }

        public async Task WhenUseCard(Card card)
        {
            if (TurnSystem.Instance.CurrentPlayer != Src) return;
            if (!card.IsConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
            await Task.Yield();
        }

        private List<Card> cards = new List<Card>();

        protected override void ResetAfterTurn()
        {
            if (TurnSystem.Instance.CurrentPlayer == Src) cards.Clear();
        }

        public override Decision AIDecision() => new Decision { action = true };
    }
}