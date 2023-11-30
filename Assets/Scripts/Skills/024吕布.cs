using GameCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 无双 : Triggered
{
    public override bool passive => true;

    protected override bool OnExecuteSha(杀 sha) => true;

    protected override async Task Invoke(Decision decision)
    {
        await Task.Yield();
        (arg as 杀).needDoubleShan = true;
    }
}

public class 利驭 : Triggered
{
    protected override bool OnMakeDamage(Damaged damaged) => damaged.SrcCard is 杀 && damaged.player.cardsCount > 0;

    private Player dest => (arg as Damaged).player;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    protected override async Task Invoke(Decision decision)
    {
        CardPanel.Instance.Title = "利驭";
        CardPanel.Instance.Hint = "对" + dest + "发动利驭，获得其一张牌";
        var card = await TimerAction.SelectOneCardFromElse(src, dest);

        // 获得牌
        await new GetCardFromElse(src, dest, card).Execute();

        // 若为装备牌
        if (card[0] is Equipment)
        {
            if (Main.Instance.AlivePlayers.Count <= 2) return;

            // 指定角色
            Timer.Instance.hint = src + "对你发动利驭，选择一名角色";
            Timer.Instance.isValidDest = player => player != src && player != dest;
            Timer.Instance.refusable = false;
            Timer.Instance.DefaultAI = AI.TryAction;
            decision = await Timer.Instance.Run(dest, 0, 1);

            // 使用决斗
            await Card.Convert<决斗>(src).UseCard(src, decision.dests);
        }
        // 摸牌
        else await new GetCardFromPile(dest, 1).Execute();
    }
}
