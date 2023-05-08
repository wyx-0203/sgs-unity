using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 千驹 : Triggered
    {
        public 千驹(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
            Src.events.AfterLoseHp.AddEvent(Src, Execute);
            Src.events.Recover.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
            Src.events.AfterLoseHp.RemoveEvent(Src);
            Src.events.Recover.RemoveEvent(Src);
            Src.DstSub -= offset;
        }

        public async Task Execute(UpdateHp updateHp)
        {
            await Task.Yield();
            Src.DstSub -= offset;
            offset = Src.HpLimit - Src.Hp;
            Src.DstSub += offset;
        }

        private int offset = 0;
    }
    public class 倾袭 : Triggered
    {
        public 倾袭(Player src) : base(src) { }

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnable()
        {
            Src.events.AfterUseCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterUseCard.RemoveEvent(Src);
        }

        public async Task Execute(Card card)
        {
            var sha = card as 杀;
            if (sha is null) return;
            foreach (var i in card.Dests)
            {
                // if (i.HandCardCount > Src.HandCardCount && i.Hp < Src.Hp) continue;

                dest = i;

                if (!await base.ShowTimer()) continue;
                Execute();

                int count = SgsMain.Instance.AlivePlayers.Where(x => DestArea.Instance.UseSha(Src, x)).Count();
                count = Mathf.Min(count, Src.weapon is null ? 2 : 4);
                Timer.Instance.Hint = "弃置" + count + "张手牌，然后弃置其武器牌，或令此牌伤害+1";
                Timer.Instance.IsValidCard = x => i.HandCards.Contains(x);
                bool result = await Timer.Instance.Run(i, count, 0);

                // if(i.isAI)
                // {

                // }

                if (result)
                {
                    await new Discard(i, Timer.Instance.Cards).Execute();
                    if (Src.weapon != null) await new Discard(Src, new List<Card> { Src.weapon }).Execute();
                }
                else
                {
                    sha.DamageValue[i.position]++;
                    var judge = await new Judge().Execute();
                    if (judge.Suit == "红桃" || judge.Suit == "方片") sha.ShanCount[i.position] = 0;
                }
            }
        }

        private Player dest;


        // protected override bool AIResult() => Src.team != dest.team;


        protected override bool AIResult()
        {
            bool result = dest.team != Src.team;
            if (result) AI.Instance.SelectDest();
            return result;
        }
    }
}