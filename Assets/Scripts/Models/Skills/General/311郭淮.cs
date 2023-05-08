using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{

    public class 精策 : Triggered
    {
        public 精策(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.End].AddEvent(Src, Execute);
            Src.events.WhenUseCard.AddEvent(Src, WhenUseCard);
            TurnSystem.Instance.AfterTurn += Reset1;
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
            Src.events.WhenUseCard.RemoveEvent(Src);
            TurnSystem.Instance.AfterTurn -= Reset1;
        }

        public new async Task Execute()
        {
            if (cards.Count < Src.Hp) return;
            if (!await base.ShowTimer()) return;
            if (cards.Find(x => x.Weight < Src.Hp) != null)
            {
                Timer.Instance.Hint = "点击确定执行一个额外的摸牌，点击取消执行出牌阶段";
                bool result = await Timer.Instance.Run(Src);
                TurnSystem.Instance.ExtraPhase.Add(result ? Phase.Get : Phase.Perform);
            }
            else
            {
                TurnSystem.Instance.ExtraPhase.Add(Phase.Get);
                TurnSystem.Instance.ExtraPhase.Add(Phase.Perform);
            }
            base.Execute();
        }

        public async Task WhenUseCard(Card card)
        {
            if (TurnSystem.Instance.CurrentPlayer != Src) return;
            if (!card.IsConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
            await Task.Yield();
        }

        private List<Card> cards = new List<Card>();

        public void Reset1()
        {
            if (TurnSystem.Instance.CurrentPlayer == Src) cards.Clear();
        }
    }
}