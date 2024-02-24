using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 仁德 : Active
{
    public override int timeLimit => int.MaxValue;

    public override int MaxCard => int.MaxValue;
    public override int MinCard => 1;
    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override bool IsValidCard(Card card) => card.isHandCard;
    public override bool IsValidDest(Player dest) => dest != src && !invalidDest.Contains(dest);

    private List<Player> invalidDest = new();
    private int count = 0;
    private bool done = false;

    public override async Task Use(PlayDecision decision)
    {
        Execute(decision);

        if (count == 0) game.turnSystem.AfterPlay += Reset;
        count += decision.cards.Count;
        invalidDest.Add(decision.dests[0]);
        await new GetAnothersCard(decision.dests[0], src, decision.cards).Execute();
        if (count < 2 || done) return;

        done = true;
        var playQuery = new PlayQuery
        {
            player = src,
            virtualCards = new List<Card>
            {
                Card.Convert<杀>(src),
                Card.Convert<火杀>(src),
                Card.Convert<雷杀>(src),
                Card.Convert<酒>(src),
                Card.Convert<桃>(src)
            },
            isValidVirtualCard = card => card.IsValid(),
            maxDestForCard = card => card.MaxDest(),
            minDestForCard = card => card.MinDest(),
            isValidDestForCard = (player, card) => card.IsValidDest(player)
        };
        playQuery.defaultAI = () =>
        {
            List<PlayDecision> decisions = new();
            var validCards = playQuery.virtualCards.Where(x => playQuery.isValidVirtualCard(x));
            foreach (var i in validCards)
            {
                var dests = new List<Player>();

                if (playQuery.maxDestForCard(i) > 0)
                {
                    dests.AddRange(game.ai.GetValidDestForCard(i));
                    if (dests.Count < playQuery.minDestForCard(i)) continue;
                }

                decisions.Add(new PlayDecision
                {
                    action = true,
                    cards = new List<Card> { i },
                    dests = dests
                });
            }

            if (decisions.Count == 0 || !AI.CertainValue) decisions.Add(new());
            // return AI.Shuffle(decisions)[0];
            return decisions.GetRandomOne();
        };

        decision = await playQuery.Run();
        if (decision.action) await decision.virtualCard.UseCard(src, decision.dests);
    }

    private void Reset()
    {
        count = 0;
        done = false;
        invalidDest.Clear();
    }

    public override PlayDecision AIDecision() => AIUseToTeammate();
}
