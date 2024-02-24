using GameCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 无双 : Triggered
{
    public override bool passive => true;

    protected override bool OnExecuteSha(杀 sha) => true;

    protected override Task Invoke(PlayDecision decision)
    {
        (arg as 杀).needDoubleShan = true;
        return Task.CompletedTask;
    }
}

public class 利驭 : Triggered
{
    protected override bool OnMakeDamage(Damage damaged) => damaged.SrcCard is 杀 && damaged.player.cardsCount > 0;

    private Player dest => (arg as Damage).player;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    protected override async Task Invoke(PlayDecision decision)
    {
        // CardPanelRequest.Instance.title = "利驭";
        string hint = $"对{dest}发动利驭，获得其一张牌";
        // var cards = await TimerAction.SelectCardFromElse(src, dest);
        var cards = await new CardPanelQuery(src, dest, name, hint, false).Run();

        // 获得牌
        await new GetAnothersCard(src, dest, cards).Execute();

        // 若为装备牌
        if (cards[0] is Equipment)
        {
            if (game.AlivePlayers.Count <= 2) return;

            // 指定角色
            // Timer.Instance.hint = src + "对你发动利驭，选择一名角色";
            // Timer.Instance.isValidDest = player => player != src && player != dest;
            // Timer.Instance.refusable = false;
            // Timer.Instance.defaultAI = AI.TryAction;
            decision = await new PlayQuery
            {
                player = dest,
                hint = $"{src}对你发动利驭，选择一名角色",
                isValidDest = player => player != src && player != dest,
                refusable = false,
                // defaultAI = AI.TryAction
            }.Run(0, 1);

            // 使用决斗
            await Card.Convert<决斗>(src).UseCard(src, decision.dests);
        }
        // 摸牌
        else await new DrawCard(dest, 1).Execute();
    }
}
