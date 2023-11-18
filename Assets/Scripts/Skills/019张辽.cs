using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 突袭 : Triggered
{
    protected override bool BeforeGetCardInGetPhase(GetCardFromPile getCardFromPile) => true;

    private GetCardFromPile getCardFromPile => arg as GetCardFromPile;

    public override int MaxDest => getCardFromPile.Count;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => dest.handCardsCount > 0 && dest != src;

    protected override async Task Invoke(Decision decision)
    {
        getCardFromPile.Count -= decision.dests.Count;
        foreach (var i in decision.dests)
        {
            CardPanel.Instance.Title = "突袭";
            CardPanel.Instance.Hint = "对" + i + "号位发动突袭，获得其一张牌";

            decision = await CardPanel.Instance.Run(src, i, i.handCards);
            await new GetCardFromElse(src, i, decision.cards).Execute();
        }
    }

    public override Decision AIDecision()
    {
        var dests = AI.GetDestByTeam(!src.team).Take(getCardFromPile.Count).ToList();
        return dests.Count > 0 ? new Decision { action = true, dests = dests } : new();
    }
}
