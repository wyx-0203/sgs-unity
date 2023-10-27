using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 恩怨 : Triggered
{
    protected override bool OnGetCardFromElse(GetCardFromElse getCardFromElse) => getCardFromElse.Cards.Count >= 2;
    protected override bool OnDamaged(Damaged damaged) => damaged.Src != src && damaged.Src != null;

    public override int MaxDest => 1;
    public override int MinDest => 1;
    public override bool IsValidDest(Player dest1) => dest1 == dest;

    private Player dest;
    private Damaged damaged;
    // private GetCardFromElse getCardFromElse;

    public override async Task Invoke(object arg)
    {
        if (arg is GetCardFromElse getCardFromElse)
        {
            dest = getCardFromElse.dest;
            var decision = await WaitDecision();
            if (!decision.action) return;
            Execute(decision);

            await new GetCardFromPile(getCardFromElse.dest, 1).Execute();
        }
        else
        {
            damaged = arg as Damaged;
            dest = damaged.Src;
            int value = damaged.value;
            for (int i = 0; i < value; i++)
            {
                var decision = await WaitDecision();
                if (!decision.action) return;
                Execute(decision);
                var dest = decision.dests[0];

                Timer.Instance.hint = "交给法正一张手牌，或失去一点体力";
                Timer.Instance.isValidCard = card => card.isHandCard;
                Timer.Instance.DefaultAI = () =>
                {
                    var cards = AI.GetRandomCard();
                    if (cards.Count == 0 || !AI.CertainValue) return new();
                    else return new Decision { action = true, cards = cards };
                };
                decision = await Timer.Instance.Run(dest, 1, 0);

                // 交手牌
                if (decision.action)
                {
                    await new GetCardFromElse(src, dest, decision.cards).Execute();
                    if (decision.cards[0].suit != "红桃") await new GetCardFromPile(src, 1).Execute();
                }
                // 失去体力
                else await new UpdateHp(dest, -1).Execute();
            }
        }
    }

    public override Decision AIDecision()
    {
        if (damaged is null && dest.team != src.team && AI.CertainValue) return new();
        return AI.TryAction();
    }
}

public class 眩惑 : Triggered
{
    protected override bool OnPhaseOver(Phase phase) => phase == Phase.Get;

    public override int MaxCard => 2;
    public override int MinCard => 2;
    public override int MaxDest => 2;
    public override int MinDest => 2;
    public override bool IsValidCard(Card card) => card.isHandCard;
    public override bool IsValidDest(Player dest) => Timer.Instance.temp.dests.Count > 0 || dest != src;

    public override async Task Invoke(object arg)
    {
        var decision = await WaitDecision();
        if (!decision.action) return;
        Execute(decision);

        var dest0 = decision.dests[0];
        var dest1 = decision.dests[1];

        // 交手牌
        await new GetCardFromElse(dest0, src, decision.cards).Execute();

        var list = new List<Card>
        {
            Card.Convert<杀>(),
            Card.Convert<火杀>(),
            Card.Convert<雷杀>(),
            Card.Convert<决斗>(),
        };
        Timer.Instance.multiConvert.AddRange(list);
        Timer.Instance.isValidCard = card => true;
        Timer.Instance.isValidDest = player => player == dest1;
        Timer.Instance.DefaultAI = () =>
        {
            if (!AI.CertainValue) return new();
            var card = AI.Shuffle(list)[0];
            return new Decision { action = true, converted = card, dests = new List<Player> { dest1 } };
        };
        decision = await Timer.Instance.Run(dest0, 0, 1);

        // 使用牌
        if (decision.action) await decision.converted.UseCard(dest0, decision.dests);
        // 交手牌
        else await new GetCardFromElse(src, dest0, new List<Card>(dest0.HandCards)).Execute();
    }

    public override Decision AIDecision()
    {
        var cards = AI.GetRandomCard();
        var dests = AI.GetDestByTeam(src.team);
        if (cards.Count < 2 || dests.Count() == 0) return new();

        var dest1 = AI.GetValidDest().Find(x => x != dests.First());
        if (dest1 is null) dest1 = src;
        return new Decision { action = true, cards = cards, dests = new List<Player> { dests.First(), dest1 } };
    }
}
