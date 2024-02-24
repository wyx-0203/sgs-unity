using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 精策 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.End && cards.Count >= src.hp;

    protected override bool OnUseCard(Card card)
    {
        if (game.turnSystem.CurrentPlayer == src)
        {
            if (cards.Count == 0) game.turnSystem.AfterTurn += () => cards.Clear();
            if (!card.isConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
        }
        return false;
    }

    protected override async Task Invoke(PlayDecision decision)
    {
        // 选择一项执行
        if (cards.Select(x => x.suit).Distinct().Count() < src.hp)
        {
            // Timer.Instance.hint = "点击确定执行一个额外的摸牌阶段，点击取消执行出牌阶段";
            // Timer.Instance.defaultAI = () => new Decision { action = AI.CertainValue };
            game.turnSystem.ExtraPhase.Add((await new PlayQuery
            {
                player = src,
                hint = "点击确定执行一个额外的摸牌阶段，点击取消执行出牌阶段",
            }.Run()).action ? Phase.Get : Phase.Play);
        }

        // 全部执行
        else
        {
            game.turnSystem.ExtraPhase.Add(Phase.Get);
            game.turnSystem.ExtraPhase.Add(Phase.Play);
        }
    }

    private List<Card> cards = new();
}
