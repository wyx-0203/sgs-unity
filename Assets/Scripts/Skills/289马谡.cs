using GameCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class 散谣 : Active
{
    public override int MaxCard => game.AlivePlayers.Where(x => x.hp == MaxHp && x != src).Count();
    public override int MinCard => 1;
    public override int MaxDest => 8;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => dest.hp == game.MaxHp(src) && dest != src;
    // 特殊类型
    public override Model.SinglePlayQuery.Type type => Model.SinglePlayQuery.Type.SanYao;

    private int MaxHp => game.MaxHp(src);

    public override async Task Use(PlayDecision decision)
    {
        decision.dests.Sort();
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        foreach (var i in decision.dests) await new Damage(i, src).Execute();
    }

    public override PlayDecision AIDecision()
    {
        var dests = AIGetDestsByTeam(~src.team);

        // // 尽量选择更多的敌人
        var cards = src.cards;
        int count = Math.Min(cards.Count(), dests.Count());

        // Timer.Instance.temp.cards = AI.Shuffle(cards, count);
        // Timer.Instance.temp.dests.AddRange(dests.Take(count));
        return new PlayDecision
        {
            cards = cards.Shuffle(count),
            dests = dests.Take(count).ToList()
        };
    }
}

public class 制蛮 : Triggered
{
    protected override bool BeforeMakeDamage(Damage damaged) => true;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    private Player dest => (arg as Damage).player;

    protected override async Task Invoke(PlayDecision decision)
    {
        // var dest = this.dest;

        if (!dest.regionIsEmpty)
        {
            // CardPanelRequest.Instance.title = "制蛮";
            string hint = $"对{dest}发动制蛮，获得其区域内一张牌";

            // var cards = await TimerAction.SelectCardFromElse(src, dest, true);
            var cards = await new CardPanelQuery(src, dest, name, hint, true).Run();
            if (dest.JudgeCards.Contains(cards[0])) await new GetJudgeCard(src, cards[0]).Execute();
            else await new GetAnothersCard(src, dest, cards).Execute();
        }
        throw new PreventDamage();
    }

    public override bool AIAct => dest.team == src.team || !dest.regionIsEmpty && new Random().NextDouble() < 0.5d;

    // public override Decision AIDecision()
    // {
    //     if (dest.team != src.team && (dest.regionIsEmpty || UnityEngine.Random.value < 0.5f) || !AI.CertainValue) return new();
    //     else return new Decision { action = true, dests = new List<Player> { dest } };
    // }
}
