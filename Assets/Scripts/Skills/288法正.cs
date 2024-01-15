using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;

public class 恩怨 : Skill.Multi
{
    public override List<Skill> skills { get; } = new List<Skill> { new _OnDamaged(), new _OnGetAnothersCard() };

    public class _OnGetAnothersCard : Triggered
    {
        protected override bool OnGetAnothersCard(GetAnothersCard getAnothersCard) => getAnothersCard.Cards.Count >= 2;

        private Player dest => (arg as GetAnothersCard).dest;

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => this.dest == dest;

        protected override async Task Invoke(PlayDecision decision)
        {
            await Task.Yield();
            await new DrawCard(dest, 1).Execute();
        }

        public override bool AIAct => dest.team == src.team;

        // public override Decision AIDecision()
        // {
        //     if (dest.team != src.team && AI.CertainValue) return new();
        //     return AI.TryAction();
        // }
    }

    public class _OnDamaged : Triggered
    {
        protected override bool OnDamagedByElse(Damage damaged) => true;

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => this.dest == dest;

        private Player dest => (arg as Damage).Src;
        private Damage damaged;

        protected override async Task Invoke(PlayDecision decision)
        {
            var dest = decision.dests[0];

            // Timer.Instance.hint = $"交给{src}一张手牌，或失去一点体力";
            // Timer.Instance.isValidCard = card => card.isHandCard;
            // Timer.Instance.defaultAI = () =>
            // {
            //     var cards = AI.GetRandomCard();
            //     if (cards.Count == 0 || !AI.CertainValue) return new();
            //     else return new Decision { action = true, cards = cards };
            // };
            decision = await new PlayQuery
            {
                player = dest,
                hint = $"交给{src}一张手牌，或失去一点体力",
                isValidCard = card => card.isHandCard,
                defaultAI = () =>
                {
                    var cards = dest.handCards.OrderBy(x => x.suit == "红桃" ^ dest.team == src.team ? -1 : 1);
                    if (cards.Count() == 0 || !AI.CertainValue) return new();
                    else return new PlayDecision { action = true, cards = new List<Card> { cards.First() } };
                }
            }.Run(1, 0);

            // 交手牌
            if (decision.action)
            {
                await new GetAnothersCard(src, dest, decision.cards).Execute();
                if (decision.cards[0].suit != "红桃") await new DrawCard(src, 1).Execute();
            }
            // 失去体力
            else await new LoseHp(dest, 1).Execute();
        }
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
    public override bool IsValidDest(Player dest) => dest != src;
    public override bool IsValidSecondDest(Player dest, Player firstDest) => true;

    protected override async Task Invoke(PlayDecision decision)
    {
        var dest0 = decision.dests[0];
        var dest1 = decision.dests[1];

        // 交手牌
        await new GetAnothersCard(dest0, src, decision.cards).Execute();

        // var list = new List<Card>
        // {
        //     Card.Convert<杀>(src),
        //     Card.Convert<火杀>(src),
        //     Card.Convert<雷杀>(src),
        //     Card.Convert<决斗>(src),
        // };
        // Timer.Instance.maxDest = () => 1;
        // Timer.Instance.minDest = () => 1;
        // Timer.Instance.isValidDest = player => player == dest1;
        // decision = await TimerAction.MultiConvert(dest0, list);
        decision = await new PlayQuery
        {
            player = dest0,
            virtualCards = new List<Card>
            {
                Card.Convert<杀>(src),
                Card.Convert<火杀>(src),
                Card.Convert<雷杀>(src),
                Card.Convert<决斗>(src),
            },
            maxDestForCard = x => 1,
            minDestForCard = x => 1,
            isValidDestForCard = (player, card) => player == dest1
        }.Run();

        // 使用牌
        if (decision.action) await decision.virtualCard.UseCard(dest0, decision.dests);
        // 交手牌
        else await new GetAnothersCard(src, dest0, dest0.handCards.ToList()).Execute();
    }

    public override PlayDecision AIDecision()
    {
        var cards = AI.Instance.GetRandomCard();
        var dest0 = AI.Instance.GetDestByTeam(src.team).FirstOrDefault();
        if (cards.Count < 2 || dest0 is null) return new();

        var dest1 = Game.Instance.AlivePlayers.Where(x => x != dest0).OrderBy(x => x.team == src.team ? -1 : 1).First();
        // if (dest1 is null) dest1 = src;
        return new PlayDecision { action = true, cards = cards, dests = new List<Player> { dest0, dest1 } };
    }
}
