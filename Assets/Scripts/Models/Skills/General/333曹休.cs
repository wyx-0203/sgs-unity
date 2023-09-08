using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 千驹 : Triggered
    {
        public override bool isObey => true;

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
            if (card is not 杀 sha) return;
            foreach (var i in card.Dests)
            {
                dest = i;

                if (!(await base.WaitDecision()).action) continue;
                await Execute();

                int count = SgsMain.Instance.AlivePlayers.Where(x => Src.DestInAttackRange(x)).Count();
                count = Mathf.Min(count, Src.weapon is null ? 2 : 4);

                Timer.Instance.hint = "弃置" + count + "张手牌，然后弃置其武器牌，或令此牌伤害+1";
                Timer.Instance.isValidCard = x => i.HandCards.Contains(x);
                Timer.Instance.AIDecision = count <= 3 ? AI.AutoDecision : () => new();

                var decision = await Timer.Instance.Run(i, count, 0);

                if (decision.action)
                {
                    await new Discard(i, decision.cards).Execute();
                    if (Src.weapon != null) await new Discard(Src, new List<Card> { Src.weapon }).Execute();
                }
                else
                {
                    sha.DamageValue[i.position]++;
                    var judge = await new Judge().Execute();
                    if (judge.isRed) sha.ShanCount[i.position] = 0;
                }
            }
        }

        private Player dest;

        public override Decision AIDecision() => new Decision { action = (dest.team != Src.team) == AI.CertainValue };
    }
}