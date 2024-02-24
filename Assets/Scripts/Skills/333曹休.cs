using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 千驹 : Triggered, Durative
{
    public override bool passive => true;

    private int offset = 0;

    protected override bool OnUpdateHp(UpdateHp updateHp)
    {
        src.subDst -= offset;
        offset = src.hpLimit - src.hp;
        src.subDst += offset;
        return false;
    }

    public void OnStart()
    {
        offset = src.hpLimit - src.hp;
        src.subDst += offset;
        OnRemove += () => src.subDst -= offset;
    }

    protected override Task Invoke(PlayDecision decision) => Task.CompletedTask;
}

public class 倾袭 : Triggered
{
    protected override bool AfterUseCard(Card card) => card is 杀 || card is 决斗;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Player dest => (arg as Card).dest;

    protected override async Task Invoke(PlayDecision decision)
    {
        var card = arg as Card;
        var dest = card.dest;

        int count = game.AlivePlayers.Where(x => src.DestInAttackRange(x)).Count();
        count = Math.Min(count, src.weapon is null ? 2 : 4);

        // Timer.Instance.hint = "弃置" + count + "张手牌，然后弃置其武器牌，或令此牌伤害+1";
        // Timer.Instance.isValidCard = x => x.isHandCard;
        // Timer.Instance.defaultAI = count <= 3 ? AI.TryAction : () => new();

        decision = await new PlayQuery
        {
            player = dest,
            hint = $"弃置{count}张手牌，然后弃置其武器牌，或令此{card}伤害+1",
            isValidCard = x => x.isHandCard,
            aiAct = count <= 3
            // defaultAI = count <= 3 ? AI.TryAction : () => new()
        }.Run(count, 0);

        if (decision.action)
        {
            // 弃手牌
            await new Discard(dest, decision.cards).Execute();
            // 弃武器
            if (src.weapon != null) await new Discard(src, new List<Card> { src.weapon }).Execute();
        }
        else
        {
            // 伤害+1
            card.AddDamageValue(dest, 1);
            var judge = await Judge.Execute(src);
            // 不可闪避
            if (judge.isRed) card.unmissableDests.Add(dest);
        }
    }

    // public override Decision AIDecision() => new Decision { action = (dest.team != src.team) == AI.CertainValue };
    public override bool AIAct => dest.team == src.team;
}
