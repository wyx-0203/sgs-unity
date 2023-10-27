using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 无双 : Triggered
{
    public override bool isObey => true;

    public override bool OnEveryUseCard(Card card) => card is 杀 && card.Src == src || card is 决斗 && (card.Src == src || card.dest == src);

    public override async Task Invoke(object arg)
    {
        await Task.Yield();
        Execute();

        if (arg is 杀 sha)
        {
            sha.shanCount = 2;
        }
        else if (arg is 决斗 juedou)
        {
            if (juedou.Src == src) juedou.DestShaCount = 2;
            else juedou.SrcShaCount = 2;
        }
    }
}

public class 利驭 : Triggered
{
    public override bool OnEveryDamaged(Damaged damaged) =>
        damaged.Src == src
        && damaged.player != src
        && damaged.SrcCard is 杀
        && damaged.player.CardCount > 0;

    private Player dest;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => this.dest == dest;

    public override async Task Invoke(object arg)
    {
        dest = (arg as Damaged).player;
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        CardPanel.Instance.Title = "利驭";
        CardPanel.Instance.Hint = "对" + dest + "发动利驭，获得其一张牌";
        var card = await TimerAction.SelectOneCard(src, dest);

        // 获得牌
        await new GetCardFromElse(src, dest, card).Execute();

        // 若为装备牌
        if (card[0] is Equipment)
        {
            if (SgsMain.Instance.AlivePlayers.Count <= 2) return;

            // 指定角色
            Timer.Instance.hint = src + "对你发动利驭，选择一名角色";
            Timer.Instance.isValidDest = player => player != src && player != dest;
            Timer.Instance.refusable = false;
            Timer.Instance.DefaultAI = AI.TryAction;
            decision = await Timer.Instance.Run(dest, 0, 1);

            // 使用决斗
            await Card.Convert<决斗>(new List<Card>()).UseCard(src, decision.dests);
        }
        // 摸牌
        else await new GetCardFromPile(dest, 1).Execute();
    }
}
