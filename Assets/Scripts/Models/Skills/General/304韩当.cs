using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 弓骑 : Active
    {
        public override int MaxCard => 1;
        public override int MinCard => 1;

        public override void OnEnable()
        {
            Src.unlimitTimes += UnlimitTimes;
        }

        public override void OnDisable()
        {
            Src.unlimitTimes -= UnlimitTimes;
        }

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);

            await new Discard(Src, decision.cards).Execute();

            Src.AttackRange += 10;
            invoked = true;
            suit = decision.cards[0].suit;

            if (decision.cards[0] is not Equipment) return;
            Timer.Instance.hint = "弃置一名其他角色的一张牌";
            Timer.Instance.isValidDest = x => x != Src && x.CardCount > 0;
            Timer.Instance.AIDecision = AI.AutoDecision;

            decision = await Timer.Instance.Run(Src, 0, 1);
            if (!decision.action) return;

            CardPanel.Instance.Title = "弓骑";
            CardPanel.Instance.Hint = "弃置其一张牌";
            var dest = decision.dests[0];
            var card = await TimerAction.SelectCard(Src, dest);
            await new Discard(dest, card).Execute();
        }

        private string suit;

        public bool UnlimitTimes(Card card) => card is 杀 && card.suit == suit && invoked;

        protected override void ResetAfterPlay()
        {
            if (invoked)
            {
                invoked = false;
                Src.AttackRange -= 10;
            }
            base.ResetAfterPlay();
        }

        private bool invoked;
    }

    public class 解烦 : Active, Ultimate
    {
        public bool IsDone { get; set; } = false;

        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override async Task Execute(Decision decision)
        {
            if (TurnSystem.Instance.Round > 1) IsDone = true;
            await base.Execute(decision);

            var dest = decision.dests[0];

            System.Func<Player, Task> func = async x =>
            {
                if (!x.DestInAttackRange(dest)) return;

                Timer.Instance.hint = "弃置一张武器牌，或令该角色摸一张牌";
                Timer.Instance.isValidCard = x => x is Weapon;

                decision = await Timer.Instance.Run(x, 1, 0);
                if (decision.action) await new Discard(x, decision.cards).Execute();
                else await new GetCardFromPile(dest, 1).Execute();
            };
            await Util.Instance.Loop(func);
        }

        public override Decision AIDecision()
        {
            // 随机指定一名队友
            var dests = AI.GetDestByTeam(Src.team).OrderBy(x => -SgsMain.Instance.AlivePlayers.Where(y => y.DestInAttackRange(x)).Count());
            Timer.Instance.temp.dests.Add(dests.First());
            return base.AIDecision();
        }
    }
}