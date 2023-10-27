using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class 千驹 : Triggered
{
    public override bool isObey => true;

    private int offset = 0;

    protected override bool OnUpdateHp(UpdateHp updateHp)
    {
        src.DstSub -= offset;
        offset = src.HpLimit - src.Hp;
        src.DstSub += offset;
        return false;
    }

    protected override void Init(string name, Player src)
    {
        base.Init(name, src);
        offset = src.HpLimit - src.Hp;
        src.DstSub += offset;
        OnRemove += () => src.DstSub -= offset;
    }
}
public class 倾袭 : Triggered
{
    protected override bool AfterSetCardTarget(Card card) => card is 杀;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest1) => dest1 == dest;

    private Player dest;

    public override async Task Invoke(object arg)
    {
        var sha = arg as 杀;
        dest = sha.dest;

        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        int count = SgsMain.Instance.AlivePlayers.Where(x => src.DestInAttackRange(x)).Count();
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
            sha.AddDamageValue(dest, 1);
            var judge = await new Judge().Execute();
            // 不可闪避
            if (judge.isRed) sha.shanCount = 0;
        }
    }

    public override Decision AIDecision() => new Decision { action = (dest.team != src.team) == AI.CertainValue };
}
