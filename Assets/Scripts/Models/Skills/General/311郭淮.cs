using Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public class 精策 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.End && cards.Count >= src.Hp;

    protected override bool OnUseCard(Card card)
    {
        if (TurnSystem.Instance.CurrentPlayer == src)
        {
            if (!card.IsConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
        }
        return false;
    }

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        // 选择一项执行
        if (cards.Select(x => x.suit).Distinct().Count() < src.Hp)
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

    protected override void ResetAfterTurn() => cards.Clear();

    public override Decision AIDecision() => new Decision { action = true };
}
