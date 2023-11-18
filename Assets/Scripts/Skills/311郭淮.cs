using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 精策 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.End && cards.Count >= src.hp;

    protected override bool OnUseCard(Card card)
    {
        if (TurnSystem.Instance.CurrentPlayer == src)
        {
            if (cards.Count == 0) TurnSystem.Instance.AfterTurn += () => cards.Clear();
            if (!card.isConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
        }
        return false;
    }

    protected override async Task Invoke(Decision decision)
    {
        // 选择一项执行
        if (cards.Select(x => x.suit).Distinct().Count() < src.hp)
        {
            Timer.Instance.hint = "点击确定执行一个额外的摸牌阶段，点击取消执行出牌阶段";
            Timer.Instance.DefaultAI = () => new Decision { action = AI.CertainValue };
            TurnSystem.Instance.ExtraPhase.Add((await Timer.Instance.Run(src)).action ? Phase.Get : Phase.Play);
        }

        // 全部执行
        else
        {
            TurnSystem.Instance.ExtraPhase.Add(Phase.Get);
            TurnSystem.Instance.ExtraPhase.Add(Phase.Play);
        }
    }

    private List<Card> cards = new();
}
