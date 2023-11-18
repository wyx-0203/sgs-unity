using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class 千驹 : Triggered, Durative
{
    public override bool isObey => true;

    private int offset = 0;

    protected override bool OnUpdateHp(UpdateHp updateHp)
    {
        src.pursueDistance -= offset;
        offset = src.hpLimit - src.hp;
        src.pursueDistance += offset;
        return false;
    }

    public void OnStart()
    {
        offset = src.hpLimit - src.hp;
        src.pursueDistance += offset;
        OnRemove += () => src.pursueDistance -= offset;
    }

    protected override async Task Invoke(Decision decision) => await Task.Yield();
}

public class 倾袭 : Triggered
{
    protected override bool AfterUseCard(Card card) => card is 杀 || card is 决斗;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Player dest => (arg as Card).dest;

    protected override async Task Invoke(Decision decision)
    {
        var card = arg as Card;
        var dest = card.dest;

        int count = Main.Instance.AlivePlayers.Where(x => src.DestInAttackRange(x)).Count();
        count = Mathf.Min(count, src.weapon is null ? 2 : 4);

        Timer.Instance.hint = "弃置" + count + "张手牌，然后弃置其武器牌，或令此牌伤害+1";
        Timer.Instance.isValidCard = x => x.isHandCard;
        Timer.Instance.DefaultAI = count <= 3 ? AI.TryAction : () => new();

        decision = await Timer.Instance.Run(dest, count, 0);

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
            var judge = await Judge.Execute();
            // 不可闪避
            if (judge.isRed) card.unmissableDests.Add(dest);
        }
    }

    public override Decision AIDecision() => new Decision { action = (dest.team != src.team) == AI.CertainValue };
}
