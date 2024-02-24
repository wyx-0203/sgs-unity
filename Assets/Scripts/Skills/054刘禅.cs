using GameCore;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 享乐 : Triggered
{
    public override bool passive => true;

    public override bool AfterEveryUseCard(Card card) => card is 杀 && card.dest == src;

    protected override async Task Invoke(PlayDecision decision)
    {
        var sha = arg as 杀;

        // Timer.Instance.hint = "请弃置一张基本牌，否则此【杀】对刘禅无效。";
        // Timer.Instance.isValidCard = x => x is BasicCard && x.discardable;
        // Timer.Instance.defaultAI = () => sha.src.team != src.team ? AI.TryAction() : new();

        decision = await new PlayQuery
        {
            player = sha.src,
            hint = $"请弃置一张基本牌，否则此【杀】对{src}无效。",
            isValidCard = x => x is BasicCard && x.discardable,
            aiAct = sha.src.team != src.team
        }.Run(1, 0);
        if (decision.action) await new Discard(sha.src, decision.cards).Execute();
        else sha.invalidDests.Add(src);
    }
}

public class 放权 : Triggered
{
    protected override bool OnPhaseStart(Phase phase) => phase == Phase.Play;

    protected override Task Invoke(PlayDecision decision)
    {
        // 出牌阶段
        // if (game.turnSystem.CurrentPhase == Phase.Play)
        // {
        // var decision = await WaitDecision();
        // if (!decision.action) return;
        // Execute(decision);

        // invoked = true;
        game.turnSystem.BeforePhaseExecute[Phase.Discard] += OnDiscardPhase;
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
        // game.turnSystem.ExtraTurnPlayer = decision1.dests[0];
    }

    private async Task OnDiscardPhase()
    {
        if (src.handCardsCount == 0) return;
        // Timer.Instance.hint = "弃置一张手牌并令一名其他角色获得一个额外的回合";
        // Timer.Instance.isValidCard = x => x.isHandCard;
        // Timer.Instance.isValidDest = x => x != src;
        // Timer.Instance.defaultAI = AISelectDest;

        var decision = await new PlayQuery
        {
            player = src,
            hint = "弃置一张手牌并令一名其他角色获得一个额外的回合",
            isValidCard = card => card.isHandCard,
            isValidDest = player => player != src,
            defaultAI = () => new PlayDecision
            {
                cards = game.ai.GetRandomCard(),
                dests = src.teammates.Where(x => x != src).Take(1).ToList()
            }
        }.Run(1, 1);
        if (!decision.action) return;
        Execute(decision);

        await new Discard(src, decision.cards).Execute();
        game.turnSystem.ExtraTurnPlayer = decision.dests[0];
    }

    // private bool invoked = false;

    // public override Decision AIDecision()
    // {
    //     if (src.team.GetAllPlayers().Count() < 2) return new();
    //     else if (src.handCardsCount - src.handCardsLimit > 2 && AI.CertainValue) return new();
    //     else return AI.TryAction();
    // }
    public override bool AIAct => src.teammates.Count > 1 && src.handCardsCount - src.handCardsLimit <= 2;

    // private Decision AISelectDest()
    // {
    //     var dests = AI.GetDestByTeam(team).Take(1);
    //     if (dests.Count() == 0 || !AI.CertainValue) return new();

    //     var cards = AI.GetRandomCard();
    //     return new Decision { action = true, cards = cards, dests = dests.ToList() };
    // }
}
