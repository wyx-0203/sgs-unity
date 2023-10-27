using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 散谣 : Active
{
    public override int MaxCard => SgsMain.Instance.AlivePlayers.Where(x => x.Hp == MaxHp && x != src).Count();
    public override int MinCard => 1;
    public override int MaxDest => Timer.Instance.temp.cards.Count;
    public override int MinDest => Timer.Instance.temp.cards.Count;

    public override bool IsValidDest(Player dest) => dest.Hp == SgsMain.Instance.MaxHp(src) && dest != src;

    private int MaxHp => SgsMain.Instance.MaxHp(src);

    public override async Task Use(Decision decision)
    {
        decision.dests.Sort();
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        foreach (var i in decision.dests) await new Damaged(i, src).Execute();
    }

    public override Decision AIDecision()
    {
        var dests = AI.GetDestByTeam(!src.team);

        // 尽量选择更多的敌人
        var cards = src.cards.ToList();
        int count = UnityEngine.Mathf.Min(cards.Count, dests.Count());

        Timer.Instance.temp.cards = AI.Shuffle(cards, count);
        Timer.Instance.temp.dests.AddRange(dests.Take(MinDest));
        return base.AIDecision();
    }
}

public class 制蛮 : Triggered
{
    public override bool BeforeEveryDamaged(Damaged damaged) => damaged.Src == src && damaged.player != src;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Player dest;

    public override async Task Invoke(object arg)
    {
        dest = (arg as Damaged).player;
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        if (!dest.RegionIsEmpty)
        {
            CardPanel.Instance.Title = "制蛮";
            CardPanel.Instance.Hint = "对" + dest + "发动制蛮，获得其区域内一张牌";

            var card = await TimerAction.SelectOneCard(src, dest, true);
            if (dest.JudgeCards.Contains(card[0])) await new GetJudgeCard(src, card[0]).Execute();
            else await new GetCardFromElse(src, dest, card).Execute();
        }
        throw new PreventDamage();
    }


    public override Decision AIDecision()
    {
        if (dest.team != src.team && (dest.RegionIsEmpty || UnityEngine.Random.value < 0.5f) || !AI.CertainValue) return new();
        else return new Decision { action = true, dests = new List<Player> { dest } };
    }
}
