using Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class 当先 : Triggered
{
    public override bool isObey => true;

    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Prepare || phase == Phase.Play && inSkill;

    private bool inSkill;
    private bool fuliIsInvoked;

    public override async Task Invoke(object arg)
    {

        // 准备阶段
        if (TurnSystem.Instance.CurrentPhase == Phase.Prepare)
        {
            Execute();
            inSkill = true;
            TurnSystem.Instance.ExtraPhase.Add(Phase.Play);
        }
        // 出牌阶段
        else
        {
            inSkill = false;
            if (fuliIsInvoked)
            {
                var decision = await WaitDecision();
                if (!decision.action) return;
            }
            Execute();

            await new UpdateHp(src, -1).Execute();
            var card = CardPile.Instance.DiscardPile.Find(x => x is 杀);
            if (card != null) await new GetDisCard(src, new List<Card> { card }).Execute();
        }
    }

    public override Decision AIDecision() => new Decision { action = src.Hp > 1 && src.FindCard<杀>() is null };
}

public class 伏枥 : Triggered, Ultimate
{
    public bool IsDone { get; set; } = false;

    // public async Task Execute()
    // {

    // }
}