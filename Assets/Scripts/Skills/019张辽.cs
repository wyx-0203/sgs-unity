using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 突袭 : Triggered
{
    protected override bool BeforeDrawInDrawPhase(DrawCard getCardFromPile) => true;

    private DrawCard drawCard => arg as DrawCard;

    public override int MaxDest => drawCard.Count;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest) => dest.handCardsCount > 0 && dest != src;

    protected override async Task Invoke(PlayDecision decision)
    {
        drawCard.Count -= decision.dests.Count;
        decision.dests.Sort();
        foreach (var i in decision.dests)
        {
            // CardPanelRequest.Instance.title = "突袭";
            string hint = $"对{i}发动突袭，获得其一张牌";

            // decision = await CardPanelRequest.Instance.Run(src, i, i.handCards);
            var cards = await new CardPanelQuery(src, i, name, hint, i.handCards).Run();
            await new GetAnothersCard(src, i, cards).Execute();
        }
    }

    // public override bool AIAct => game.players.FirstOrDefault(x => IsValidDest(x) && x.team != src.team) != null;

    public override PlayDecision AIDecision() => new PlayDecision
    {
        dests = game.AlivePlayers
            .Where(x => IsValidDest(x) && x.team != src.team)
            .Take(drawCard.Count)
            .ToList()
    };
}
