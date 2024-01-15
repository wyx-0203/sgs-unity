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

        if (count == 0) TurnSystem.Instance.AfterPlay += Reset;
        count += decision.cards.Count;
        invalidDest.Add(decision.dests[0]);
        await new GetAnothersCard(decision.dests[0], src, decision.cards).Execute();
        if (count < 2 || done) return;

        done = true;
        // var list = new List<Card>
        // {
        //     Card.Convert<杀>(src),
        //     Card.Convert<火杀>(src),
        //     Card.Convert<雷杀>(src),
        //     Card.Convert<酒>(src),
        //     Card.Convert<桃>(src)
        // };

        // Timer.Instance.maxDest = DestArea.Instance.MaxDest;
        // Timer.Instance.minDest = DestArea.Instance.MinDest;
        // Timer.Instance.isValidDest = DestArea.Instance.ValidDest;
        // Timer.Instance.defaultAI = () =>
        // {
        //     List<Decision> decisions = new();
        //     var validCards = Timer.Instance.multiConvert.Where(x => Timer.Instance.isValidCard(x)).ToList();
        //     foreach (var i in validCards)
        //     {
        //         Timer.Instance.temp.cards.Add(i);

        //         if (Timer.Instance.maxDest() > 0)
        //         {
        //             var dests = AI.GetValidDest();
        //             if (dests is null || dests[0].team == src.team) continue;

        //             Timer.Instance.temp.dests.AddRange(dests);
        //         }

        //         Timer.Instance.temp.action = true;
        //         decisions.Add(Timer.Instance.SaveTemp());
        //         Timer.Instance.temp.cards.Remove(i);
        //     }

        //     if (decisions.Count == 0 || !AI.CertainValue) decisions.Add(new Decision());
        //     return AI.Shuffle(decisions)[0];
        // };

        // decision = await TimerAction.MultiConvert(src, list, CardArea.Instance.ValidCard);
        decision = await new PlayQuery
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
            isValidDestForCard = (player, card) => card.IsValidDest(player),
            defaultAI = () => new()
            // {
            //     List<Decision> decisions = new();
            //     var validCards = Timer.Instance.multiConvert.Where(x => Timer.Instance.isValidCard(x)).ToList();
            //     foreach (var i in validCards)
            //     {
            //         Timer.Instance.temp.cards.Add(i);

            //         if (Timer.Instance.maxDest() > 0)
            //         {
            //             var dests = AI.GetValidDest();
            //             if (dests is null || dests[0].team == src.team) continue;

            //             Timer.Instance.temp.dests.AddRange(dests);
            //         }

            //         Timer.Instance.temp.action = true;
            //         decisions.Add(Timer.Instance.SaveTemp());
            //         Timer.Instance.temp.cards.Remove(i);
            //     }

            //     if (decisions.Count == 0 || !AI.CertainValue) decisions.Add(new Decision());
            //     return AI.Shuffle(decisions)[0];
            // }
        }.Run();
        if (decision.action) await decision.virtualCard.UseCard(src, decision.dests);
    }

    private void Reset()
    {
        count = 0;
        done = false;
        invalidDest.Clear();
    }

    public override PlayDecision AIDecision()
    {
        // Timer.Instance.temp.dests = AI.GetDestByTeam(src).Take(1).ToList();
        // Timer.Instance.temp.cards = AI.GetRandomCard();
        return base.AIDecision();
    }
}
