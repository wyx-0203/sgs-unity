using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 驱虎 : Active
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => dest.Hp > Src.Hp && dest.HandCardCount > 0;

        public override bool IsValid => base.IsValid && Src.HandCardCount > 0;

        public override async Task Execute(Decision decision)
        {
            await base.Execute(decision);
            var dest = decision.dests[0];

            // 拼点没赢
            if (!await new Compete(Src, dest).Execute())
            {
                await new Damaged(Src, decision.dests[0]).Execute();
                return;
            }

            // 攻击范围内没有角色，直接返回
            if (SgsMain.Instance.AlivePlayers.Find(x => dest.DestInAttackRange(x)) is null) return;

            Timer.Instance.hint = "请选择一名角色";
            Timer.Instance.refusable = false;
            Timer.Instance.isValidDest = x => dest.DestInAttackRange(x);
            Timer.Instance.DefaultAI = AI.TryAction;

            decision = await Timer.Instance.Run(Src, 0, 1);
            await new Damaged(decision.dests[0], dest).Execute();
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetValidDest();
            Timer.Instance.temp.dests.AddRange(dests.Take(1));
            return base.AIDecision();
        }
    }

    public class 节命 : Triggered
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;

        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
        }

        public async Task Execute(Damaged damaged)
        {
            for (int i = 0; i < -damaged.Value; i++)
            {
                var decision = await WaitDecision();
                if (!decision.action) return;
                await Execute(decision);

                var dest = decision.dests[0];
                int count = dest.HpLimit - dest.HandCardCount;
                if (count > 0) await new GetCardFromPile(dest, count).Execute();
            }
        }

        public override Decision AIDecision()
        {
            var dests = AI.GetDestByTeam(Src.team).OrderBy(x => x.HandCardCount).Take(1);
            return dests.Count() > 0 ? new Decision { action = true, dests = dests.ToList() } : new();
        }
    }
}