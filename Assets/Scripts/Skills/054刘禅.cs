using GameCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class 享乐 : Triggered
{
    public override bool passive => true;

    public override bool AfterEveryUseCard(Card card) => card is 杀 && card.dest == src;

    protected override async Task Invoke(Decision decision)
    {
        var sha = arg as 杀;

        Timer.Instance.hint = "请弃置一张基本牌，否则此【杀】对刘禅无效。";
        Timer.Instance.isValidCard = x => x is BasicCard && x.discardable;
        Timer.Instance.DefaultAI = () => sha.src.team != src.team ? AI.TryAction() : new(); ;

        decision = await Timer.Instance.Run(sha.src, 1, 0);
        if (decision.action) await new Discard(sha.src, decision.cards).Execute();
        else sha.invalidDests.Add(src);
    }
}

public class 放权 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Play;

    protected override async Task Invoke(Decision decision)
    {
        // 出牌阶段
        // if (TurnSystem.Instance.CurrentPhase == Phase.Play)
        // {
        // var decision = await WaitDecision();
        // if (!decision.action) return;
        // Execute(decision);

        // invoked = true;
        await Task.Yield();
        TurnSystem.Instance.BeforePhaseExecute[Phase.Discard] += OnDiscardPhase;
        throw new SkipPhaseException();
        // }

        // 弃牌阶段

        // invoked = false;

        // Timer.Instance.hint = "弃置一张手牌并令一名其他角色获得一个额外的回合";
        // Timer.Instance.isValidCard = x => x.isHandCard;
        // Timer.Instance.isValidDest = x => x != src;
        // Timer.Instance.DefaultAI = AISelectDest;

        // var decision1 = await Timer.Instance.Run(src, 1, 1);
        // if (!decision1.action) return;
        // Execute(decision1);

        // await new Discard(src, decision1.cards).Execute();
        // TurnSystem.Instance.ExtraTurnPlayer = decision1.dests[0];
    }

    private async Task OnDiscardPhase()
    {
        if (src.handCardsCount == 0) return;
        Timer.Instance.hint = "弃置一张手牌并令一名其他角色获得一个额外的回合";
        Timer.Instance.isValidCard = x => x.isHandCard;
        Timer.Instance.isValidDest = x => x != src;
        Timer.Instance.DefaultAI = AISelectDest;

        var decision = await Timer.Instance.Run(src, 1, 1);
        if (!decision.action) return;
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        TurnSystem.Instance.ExtraTurnPlayer = decision.dests[0];
    }

    // private bool invoked = false;

    public override Decision AIDecision()
    {
        if (src.team.GetAllPlayers().Count() < 2) return new();
        else if (src.handCardsCount - src.handCardsLimit > 2 && AI.CertainValue) return new();
        else return AI.TryAction();
    }

    private Decision AISelectDest()
    {
        var dests = AI.GetDestByTeam(src.team).Take(1);
        if (dests.Count() == 0 || !AI.CertainValue) return new();

        var cards = AI.GetRandomCard();
        return new Decision { action = true, cards = cards, dests = dests.ToList() };
    }
}
