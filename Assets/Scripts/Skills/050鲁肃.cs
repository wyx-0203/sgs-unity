using GameCore;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class 好施 : Triggered
{
    protected override bool BeforeDrawInDrawPhase(DrawCard getCardFromPile) => true;

    protected override async Task Invoke(PlayDecision decision)
    {
        await Task.Yield();
        var getCardFromPile = arg as DrawCard;
        getCardFromPile.Count += 2;
        getCardFromPile.afterExecute += Give;
    }

    public async Task Give()
    {
        if (src.handCardsCount <= 5) return;
        int count = src.handCardsCount / 2;
        int min = Game.Instance.MinHandCard(src);

        // Timer.Instance.hint = "请选择" + count + "张手牌，交给一名手牌最少的角色";
        // Timer.Instance.isValidDest = dest => dest.handCardsCount == min;
        // Timer.Instance.isValidCard = card => card.isHandCard;
        // Timer.Instance.refusable = false;
        // Timer.Instance.defaultAI = () => new Decision
        // {
        //     action = true,
        //     cards = AI.GetRandomCard(),
        //     dests = AI.GetAllDests().OrderBy(x => x.team == src.team ? -1 : 1).Take(1).ToList(),
        // };

        var decision = await new PlayQuery
        {
            player = src,
            hint = $"请选择{count}张手牌，交给一名手牌最少的角色",
            isValidDest = dest => dest.handCardsCount == min,
            isValidCard = card => card.isHandCard,
            refusable = false,
            defaultAI = () => new PlayDecision
            {
                action = true,
                cards = AI.Instance.GetRandomCard(),
                dests = AI.Instance.GetValidDest()
                    .OrderBy(x => x.team == src.team ? -1 : 1)
                    .Take(1)
                    .ToList()
            }
        }.Run(count, 1);
        await new GetAnothersCard(decision.dests[0], src, decision.cards).Execute();
    }

    public override bool AIAct => src.handCardsCount <= 1
        || src.teammates.FirstOrDefault(x => x.handCardsCount == Game.Instance.MinHandCard(src) && x != src) != null;
    // public override Decision AIDecision()
    // {
    //     int min = Game.Instance.MinHandCard(src);
    //     return new Decision { action = src.handCardsCount <= 1 || src.teammates.FirstOrDefault(x => x.handCardsCount == min && x != src)!=null};
    // }
}

public class 缔盟 : Active
{
    public override int MaxDest => 2;
    public override int MinDest => 2;
    public override bool IsValidDest(Player dest) => src != dest;
    public override bool IsValidSecondDest(Player dest, Player firstDest) =>
        Mathf.Abs(firstDest.handCardsCount - dest.handCardsCount) <= src.cardsCount;

    public override async Task Use(PlayDecision decision)
    {
        decision.dests.Sort();
        Execute(decision);

        // 弃牌
        int count = Mathf.Abs(decision.dests[0].handCardsCount - decision.dests[1].handCardsCount);
        if (count > 0)
        {
            // Timer.Instance.hint = $"请弃置{count}张牌";
            // Timer.Instance.refusable = false;
            // Timer.Instance.defaultAI = AI.TryAction;
            var _decision = await new PlayQuery
            {
                player = src,
                hint = $"请弃置{count}张牌",
                refusable = false,
            }.Run(count, 0);
            await new Discard(src, _decision.cards).Execute();
        }

        // 交换手牌
        await new ExChange(decision.dests[0], decision.dests[1]).Execute();
    }
    public override PlayDecision AIDecision()
    {
        // 将队友按手牌数量生序排列，敌人降序排列
        // var teammates = Game.Instance.AlivePlayers.Where(x => x.team == src.team && x != src).OrderBy(x => x.handCardsCount).ToArray();
        // var dests = (!src.team).GetAllPlayers().OrderBy(x => -x.handCardsCount).ToArray();

        // if (teammates.Count() == 0) return new();
        // int i = 0, j = 0, diff = dests[j].handCardsCount - teammates[i].handCardsCount;

        // // 找到手牌相差最大的情况
        // while (diff > src.cardsCount && (i < teammates.Length - 1 || j < dests.Length - 1))
        // {
        //     if (i == teammates.Length - 1) j++;
        //     else if (j == dests.Length - 1) i++;
        //     else if (teammates[i + 1].handCardsCount - teammates[i].handCardsCount < dests[j].handCardsCount - dests[j + 1].handCardsCount) i++;
        //     else j++;
        //     diff = dests[j].handCardsCount - teammates[i].handCardsCount;
        // }

        // if (diff < 0 || diff > src.handCardsCount) return new();
        // Timer.Instance.temp.dests.Add(teammates[i]);
        // Timer.Instance.temp.dests.Add(dests[j]);
        return base.AIDecision();
    }
}