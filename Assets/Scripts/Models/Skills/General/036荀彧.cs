using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 驱虎 : Active
    {
        public 驱虎(Player src) : base(src) { }

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest) => dest.Hp > Src.Hp && dest.HandCardCount > 0;

        public override bool IsValid => base.IsValid && Src.HandCardCount > 0;

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            await base.Execute(dests, cards, other);

            // var compete = new Compete(Src, dests[0]);
            // await compete.Execute();
            // if (compete.Result)
            if (await new Compete(Src, dests[0]).Execute())
            {
                // Debug.Log("win");
                if (SgsMain.Instance.AlivePlayers.Find(x => DestArea.Instance.UseSha(dests[0], x)) is null) return;
                Timer.Instance.Hint = "请选择一名角色";
                Timer.Instance.Refusable = false;
                Timer.Instance.IsValidDest = dest => DestArea.Instance.UseSha(dests[0], dest);
                await Timer.Instance.Run(Src, 0, 1);
                await new Damaged(Timer.Instance.Dests[0], dests[0]).Execute();
            }
            else await new Damaged(Src, dests[0]).Execute();
        }
    }

    public class 节命 : Triggered
    {
        public 节命(Player src) : base(src) { }

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
                if (!await base.ShowTimer()) return;
                Execute();
                var dest = Timer.Instance.Dests[0];
                int count = dest.HpLimit - dest.HandCardCount;
                if (count > 0) await new GetCardFromPile(dest, count).Execute();
            }
        }

        protected override bool AIResult()
        {
            // 所有队友
            var teammates = SgsMain.Instance.AlivePlayers.Where(x => x.team == Src.team).ToList();
            // 按摸牌数量排序
            teammates.Sort((x, y) => (x.HpLimit - x.HandCardCount) > (y.HpLimit - y.HandCardCount) ? -1 : 1);
            Operation.Instance.Dests.Add(Src.Teammates[0]);
            Operation.Instance.AICommit();
            return true;
        }
    }
}