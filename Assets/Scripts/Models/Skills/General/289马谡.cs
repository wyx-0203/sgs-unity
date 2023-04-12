using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 散谣 : Active
    {
        public 散谣(Player src) : base(src) { }

        public override int MaxCard => SgsMain.Instance.AlivePlayers.Where(x => x.Hp == MaxHp && x != Src).Count();
        public override int MinCard => 1;
        public override int MaxDest => Operation.Instance.Cards.Count + Operation.Instance.Equips.Count;
        public override int MinDest => Operation.Instance.Cards.Count + Operation.Instance.Equips.Count;

        public override bool IsValidDest(Player dest)
        {
            return dest.Hp == SgsMain.Instance.MaxHp(Src) && dest != Src;
        }

        private int MaxHp => SgsMain.Instance.MaxHp(Src);

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            TurnSystem.Instance.SortDest(dests);
            await base.Execute(dests, cards, other);

            await new Discard(Src, cards).Execute();
            foreach (var i in dests) await new Damaged(i, Src).Execute();
        }
    }

    public class 制蛮 : Triggered
    {
        public 制蛮(Player src) : base(src) { }

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
            if (!await base.ShowTimer()) return;
            Execute();
            damaged.Value = 0;
            if (!damaged.player.RegionHaveCard) return;

            CardPanel.Instance.Title = "制蛮";
            CardPanel.Instance.Hint = "对" + dest.posStr + "号位发动制蛮，获得其区域内一张牌";
            var card = await CardPanel.Instance.SelectCard(Src, damaged.player, true);
            if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
            {
                await new GetJudgeCard(Src, card).Execute();
            }
            else await new GetCardFromElse(Src, damaged.player, new List<Card> { card }).Execute();
        }

        private Player dest;

        protected override bool AIResult()
        {
            AI.Instance.SelectDest();
            return dest.team == Src.team || dest.Equipages.Values.ToList().Find(x => x != null) != null;
        }
    }
}