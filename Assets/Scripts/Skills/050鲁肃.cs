using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore;

public class 好施 : Triggered
{
    protected override bool BeforeDrawInDrawPhase(DrawCard getCardFromPile) => true;

    protected override Task Invoke(PlayDecision decision)
    {
        var getCardFromPile = arg as DrawCard;
        getCardFromPile.Count += 2;
        getCardFromPile.afterExecute += Give;
        return Task.CompletedTask;
    }

    public async Task Give()
    {
        if (src.handCardsCount <= 5) return;
        int count = src.handCardsCount / 2;
        int min = game.MinHandCard(src);

        var decision = await new PlayQuery
        {
            player = src,
            hint = $"请选择{count}张手牌，交给一名手牌最少的角色",
            isValidDest = dest => dest.handCardsCount == min,
            isValidCard = card => card.isHandCard,
            refusable = false,
            defaultAI = () => new PlayDecision
            {
                cards = game.ai.GetRandomCard(),
                dests = game.ai.GetValidDest()
                    .OrderBy(x => x.team == src.team ? -1 : 1)
                    .Take(1)
                    .ToList()
            }
        }.Run(count, 1);
        await new GetAnothersCard(decision.dests[0], src, decision.cards).Execute();
    }

    public override bool AIAct => src.handCardsCount <= 1
        || src.teammates.FirstOrDefault(x => x.handCardsCount == game.MinHandCard(src) && x != src) != null;
}

public class 缔盟 : Active
{
    public override int MaxDest => 2;
    public override int MinDest => 2;
    public override bool IsValidDest(Player dest) => src != dest;
    public override bool IsValidSecondDest(Player dest, Player firstDest) =>
        Math.Abs(firstDest.handCardsCount - dest.handCardsCount) <= src.cardsCount;

    public override async Task Use(PlayDecision decision)
    {
        decision.dests.Sort();
        Execute(decision);

        // 弃牌
        int count = Math.Abs(decision.dests[0].handCardsCount - decision.dests[1].handCardsCount);
        if (count > 0)
        {
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

    private int Diff(Player player0, Player player1) => player0.handCardsCount - player1.handCardsCount;

    public override PlayDecision AIDecision()
    {
        // 将队友按手牌数量生序排列，敌人降序排列
        var teammates = AIGetDestsByTeam(src.team).ToArray();
        var enemys = AIGetDestsByTeam(~src.team).ToArray();

        if (teammates.Length == 0 || enemys.Length == 0) return new();
        Player dest0 = teammates[0], dest1 = enemys[0];
        int t = src.cardsCount;

        // 找到手牌相差最大的情况
        for (int i = 0; i < teammates.Length; i++)
        {
            for (int j = 0; j < enemys.Length; j++)
            {
                int diff = Diff(enemys[j], teammates[i]);
                if (diff <= t && diff > Diff(dest1, dest0))
                {
                    dest0 = teammates[i];
                    dest1 = enemys[j];
                }
            }
        }

        if (Diff(dest1, dest0) > t || Diff(dest1, dest0) <= 0) return new();
        return new PlayDecision
        {
            cards = AIGetCards(),
            dests = new List<Player> { dest0, dest1 }
        };
    }
}