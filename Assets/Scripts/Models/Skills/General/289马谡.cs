using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class 散谣 : Active
    {
        public override int MaxCard => SgsMain.Instance.AlivePlayers.Where(x => x.Hp == MaxHp && x != Src).Count();
        public override int MinCard => 1;
        public override int MaxDest => Timer.Instance.temp.cards.Count;
        public override int MinDest => Timer.Instance.temp.cards.Count;

        public override bool IsValidDest(Player dest) => dest.Hp == SgsMain.Instance.MaxHp(Src) && dest != Src;

        private int MaxHp => SgsMain.Instance.MaxHp(Src);

        public override async Task Execute(Decision decision)
        {
            TurnSystem.Instance.SortDest(decision.dests);
            await base.Execute(decision);

            await new Discard(Src, decision.cards).Execute();
            foreach (var i in decision.dests) await new Damaged(i, Src).Execute();
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetDestByTeam(!Src.team);

            // 尽量选择更多的敌人
            var cards = Src.cards.ToList();
            int count = UnityEngine.Mathf.Min(cards.Count, dests.Count());

            Timer.Instance.temp.cards = AI.Shuffle(cards, count);
            Timer.Instance.temp.dests.AddRange(dests.Take(MinDest));
            return base.AIDecision();
        }
    }

    public class 制蛮 : Triggered
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.WhenDamaged.AddEvent(Src, Execute);
            }
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.WhenDamaged.RemoveEvent(Src);
            }
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null || damaged.Src != Src) return;
            dest = damaged.player;

            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            if (!damaged.player.RegionIsEmpty)
            {
                CardPanel.Instance.Title = "制蛮";
                CardPanel.Instance.Hint = "对" + dest.posStr + "号位发动制蛮，获得其区域内一张牌";

                var card = await TimerAction.SelectOneCard(Src, damaged.player, true);
                if (dest.JudgeCards.Contains(card[0])) await new GetJudgeCard(Src, card[0]).Execute();
                else await new GetCardFromElse(Src, damaged.player, card).Execute();
            }
            throw new PreventDamage();
        }

        private Player dest;

        public override Decision AIDecision()
        {
            if (dest.team != Src.team && (dest.RegionIsEmpty || UnityEngine.Random.value < 0.5f) || !AI.CertainValue) return new();
            else return new Decision { action = true, dests = new List<Player> { dest } };
        }
    }
}