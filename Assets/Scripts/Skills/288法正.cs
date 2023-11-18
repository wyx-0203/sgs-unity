using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 恩怨 : Skill.Multi
{
    public override List<Skill> skills => new List<Skill> { new _OnDamaged(), new _OnGetCardFromElse() };

    public class _OnGetCardFromElse : Triggered
    {
        protected override bool OnGetCardFromElse(GetCardFromElse getCardFromElse) => getCardFromElse.Cards.Count >= 2;

        private Player dest => (arg as GetCardFromElse).dest;

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => this.dest == dest;

        protected override async Task Invoke(Decision decision)
        {
            await Task.Yield();
            await new GetCardFromPile(dest, 1).Execute();
        }

        public override Decision AIDecision()
        {
            if (dest.team != src.team && AI.CertainValue) return new();
            return AI.TryAction();
        }
    }

    public class _OnDamaged : Triggered
    {
        protected override bool OnDamagedByElse(Damaged damaged) => true;

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => this.dest == dest;

        private Player dest => (arg as Damaged).Src;
        private Damaged damaged;

        protected override async Task Invoke(Decision decision)
        {
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

public class 眩惑 : Triggered
{
    protected override bool OnPhaseOver(Phase phase) => phase == Phase.Get;

    public override int MaxCard => 2;
    public override int MinCard => 2;
    public override int MaxDest => 2;
    public override int MinDest => 2;
    public override bool IsValidCard(Card card) => card.isHandCard;
    public override bool IsValidDest(Player dest) => Timer.Instance.temp.dests.Count > 0 || dest != src;

    protected override async Task Invoke(Decision decision)
    {
        var dest0 = decision.dests[0];
        var dest1 = decision.dests[1];

        // 交手牌
        await new GetCardFromElse(dest0, src, decision.cards).Execute();

        var list = new List<Card>
        {
            Card.Convert<杀>(src),
            Card.Convert<火杀>(src),
            Card.Convert<雷杀>(src),
            Card.Convert<决斗>(src),
        };
        Timer.Instance.maxDest = () => 1;
        Timer.Instance.minDest = () => 1;
        Timer.Instance.isValidDest = player => player == dest1;
        decision = await TimerAction.MultiConvert(dest0, list);

        // 使用牌
        if (decision.action) await decision.cards[0].UseCard(dest0, decision.dests);
        // 交手牌
        else await new GetCardFromElse(src, dest0, dest0.handCards.ToList()).Execute();
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
