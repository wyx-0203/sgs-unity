using Model;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class 好施 : Triggered
{
    // 摸牌前触发
    protected override bool BeforeGetCardInGetPhase(GetCardFromPile getCardFromPile) => true;

    // 摸牌后触发
    protected override bool OnGetCard(GetCard getCard) =>
        getCard is GetCardFromPile getCardFromPile
        && getCardFromPile.inGetPhase
        && invoked
        && src.HandCardCount > 5;

    private bool invoked;

    public override async Task Invoke(object arg)
    {
        // 额外摸两张牌
        if (!invoked)
        {
            var decision = await WaitDecision();
            if (!decision.action) return;
            Execute(decision);

            invoked = true;
            var getCardFromPile = arg as GetCardFromPile;
            getCardFromPile.Count += 2;
        }
        // 交给其他角色
        else await Give();
    }

    public async Task Give()
    {
        int count = src.HandCardCount / 2;
        int min = SgsMain.Instance.MinHandCard(src);

        Timer.Instance.hint = "请选择" + count + "张手牌，交给一名手牌最少的角色";
        Timer.Instance.isValidDest = dest => dest.HandCardCount == min;
        Timer.Instance.isValidCard = card => card.isHandCard;
        Timer.Instance.refusable = false;
        Timer.Instance.DefaultAI = () => new Decision
        {
            action = true,
            cards = AI.GetRandomCard(),
            dests = AI.GetAllDests().OrderBy(x => x.team == src.team ? -1 : 1).Take(1).ToList(),
        };

        var decision = await Timer.Instance.Run(src, count, 1);
        await new GetCardFromElse(decision.dests[0], src, decision.cards).Execute();
    }

    protected override void ResetAfterTurn()
    {
        invoked = false;
    }

    public override Decision AIDecision()
    {
        int min = SgsMain.Instance.MinHandCard(src);
        return new Decision { action = src.HandCardCount <= 1 || src.team.GetAllPlayers().Where(x => x.HandCardCount == min && x != src).Count() > 0 };
    }
}

public class 缔盟 : Active
{
    public override int MaxDest => 2;
    public override int MinDest => 2;
    public override bool IsValidDest(Player dest)
    {
        if (src == dest) return false;
        return firstDest is null || Mathf.Abs(firstDest.HandCardCount - dest.HandCardCount) <= src.CardCount;
    }

    public override async Task Use(Decision decision)
    {
        decision.dests.Sort();
        Execute(decision);

        // 弃牌
        int count = Mathf.Abs(decision.dests[0].HandCardCount - decision.dests[1].HandCardCount);
        if (count > 0)
        {
            Timer.Instance.hint = "请弃置" + count + "张牌";
            Timer.Instance.refusable = false;
            Timer.Instance.DefaultAI = AI.TryAction;
            var _decision = await Timer.Instance.Run(src, count, 0);
            await new Discard(src, _decision.cards).Execute();
        }

        // 交换手牌
        await new ExChange(decision.dests[0], decision.dests[1]).Execute();
    }
    public override Decision AIDecision()
    {
        // 将队友按手牌数量生序排列，敌人降序排列
        var teammates = SgsMain.Instance.AlivePlayers.Where(x => x.team == src.team && x != src).OrderBy(x => x.HandCardCount).ToArray();
        var dests = (!src.team).GetAllPlayers().OrderBy(x => -x.HandCardCount).ToArray();

        if (teammates.Count() == 0) return new();
        int i = 0, j = 0, diff = dests[j].HandCardCount - teammates[i].HandCardCount;

        // 找到手牌相差最大的情况
        while (diff > src.CardCount && (i < teammates.Length - 1 || j < dests.Length - 1))
        {
            if (i == teammates.Length - 1) j++;
            else if (j == dests.Length - 1) i++;
            else if (teammates[i + 1].HandCardCount - teammates[i].HandCardCount < dests[j].HandCardCount - dests[j + 1].HandCardCount) i++;
            else j++;
            diff = dests[j].HandCardCount - teammates[i].HandCardCount;
        }

        if (diff < 0 || diff > src.HandCardCount) return new();
        Timer.Instance.temp.dests.Add(teammates[i]);
        Timer.Instance.temp.dests.Add(dests[j]);
        return base.AIDecision();
    }
}