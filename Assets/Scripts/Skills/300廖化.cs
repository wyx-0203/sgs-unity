using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 当先 : Triggered
{
    public override bool passive => true;
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Prepare;

    protected override async Task Invoke(PlayDecision decision)
    {
        await Task.Yield();
        TurnSystem.Instance.ExtraPhase.Add(Phase.Play);
        TurnSystem.Instance.BeforePhaseExecute[Phase.Play] += async () =>
        {
            if (src.FindSkill<伏枥>().IsDone)
            {
                var decision = await WaitDecision();
                if (!decision.action) return;
            }
            // Execute();

            await new LoseHp(src, 1).Execute();
            var card = CardPile.Instance.DiscardPile.Find(x => x is 杀);
            if (card != null) await new GetDiscard(src, new List<Card> { card }).Execute();
        };
    }

    // public override PlayDecision AIDecision() => new  PlayDecision { action = src.hp > 1 && src.FindCard<杀>() is null };
    public override bool AIAct => src.hp > 1 && src.FindCard<杀>() is null;
}

public class 伏枥 : Triggered, Limited
{
    public bool IsDone { get; set; } = false;
    protected override bool OnNearDeath() => true;

    protected override async Task Invoke(PlayDecision decision)
    {
        IsDone = true;
        // 势力数
        int count = Game.Instance.AlivePlayers.Select(x => x.general.nation).Distinct().Count();
        // 回复体力
        await new Recover(src, count - src.hp).Execute();
        // 摸牌
        if (count - src.handCardsCount > 0) await new DrawCard(src, count - src.handCardsCount).Execute();
        // 翻面
        if (count >= 3) await new TurnOver(src).Execute();
    }
}