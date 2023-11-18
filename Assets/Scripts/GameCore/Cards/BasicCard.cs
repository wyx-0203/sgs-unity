using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameCore
{
    public class BasicCard : Card { }

    public class 杀 : BasicCard
    {
        public 杀()
        {
            type = "基本牌";
            name = "杀";
        }

        protected override async Task BeforeUse()
        {
            // shanCount = 1;
            needDoubleShan = false;
            ignoreArmor = false;

            // 青釭剑 朱雀羽扇
            if (src.weapon != null) await src.weapon.BeforeUseSha(this);

            // 出杀次数+1
            src.shaCount++;
            TurnSystem.Instance.AfterPlay += () => src.shaCount = 0;

            // 酒
            if (src.useJiu)
            {
                src.useJiu = false;
                for (int i = 0; i < damageOffset.Length; i++) damageOffset[i]++;
            }
        }

        protected override async Task AfterInit()
        {
            // 雌雄双股剑
            if (src.weapon != null) await src.weapon.AfterInitSha(this);
        }

        protected override async Task UseForeachDest()
        {
            // 杀生效时事件 (无双，肉林)
            await Triggered.Invoke(x => x.OnEveryExecuteSha, this);

            isDamage =
                // 不可闪避
                unmissableDests.Contains(dest)
                || src.effects.Unmissable.Invoke(this, dest)
                // 使用闪
                || !await 闪.Call(dest, this)
                // 需要两张闪
                || needDoubleShan && !await 闪.Call(dest, this);

            // 青龙偃月刀，贯石斧
            if (!isDamage && src.weapon != null) await src.weapon.OnShaMissed(this);

            if (isDamage)
            {
                // 寒冰剑
                try { if (src.weapon != null) await src.weapon.OnShaDamage(this); }
                catch (PreventDamage) { return; }

                Damaged.Type type = this is 火杀 ? Damaged.Type.Fire : this is 雷杀 ? Damaged.Type.Thunder : Damaged.Type.Normal;
                await new Damaged(dest, src, this, 1 + damageOffset[dest.position], type).Execute();
            }
        }

        public bool needDoubleShan { get; set; }
        public bool ignoreArmor { get; set; }
        public bool isDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            Timer.Instance.hint = "请打出一张杀。";
            Timer.Instance.isValidCard = card => card.Useable<杀>();

            var decision = await Timer.Instance.Run(player, 1, 0);
            if (!decision.action) return false;

            await decision.cards[0].Put(player);
            return true;
        }
    }

    public class 闪 : BasicCard
    {
        public 闪()
        {
            type = "基本牌";
            name = "闪";
        }

        public static async Task<bool> Call(Player player, Card srcCard)
        {
            if (player.armor is 八卦阵 baguazhen)
            {
                bool result = await baguazhen.Invoke(srcCard);
                if (result) return true;
            }

            Timer.Instance.hint = "请使用一张闪。";
            Timer.Instance.isValidCard = card => card.Useable<闪>();
            Timer.Instance.DefaultAI = AI.TryAction;

            var decision = await Timer.Instance.Run(player, 1, 0);

            if (!decision.action) return false;

            await decision.cards[0].UseCard(player);
            return true;
        }
    }

    public class 桃 : BasicCard
    {
        public 桃()
        {
            type = "基本牌";
            name = "桃";
        }

        protected override async Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            await new Recover(dest).Execute();
        }

        public static async Task<bool> Call(Player player, Player dest)
        {
            Timer.Instance.hint = "请使用一张桃。";
            Timer.Instance.isValidCard = card => (card is 桃 || card is 酒 && dest == player) && card.useable;
            Timer.Instance.isValidDest = player => player == dest;
            Timer.Instance.DefaultAI = player.team == dest.team ? AI.TryAction : () => new();

            var decision = await Timer.Instance.Run(player, 1, 1);
            if (!decision.action) return false;

            await decision.cards[0].UseCard(player, new List<Player> { dest });
            return true;
        }
    }

    public class 火杀 : 杀
    {
        public 火杀()
        {
            type = "基本牌";
            name = "火杀";
        }
    }

    public class 雷杀 : 杀
    {
        public 雷杀()
        {
            type = "基本牌";
            name = "雷杀";
        }
    }

    public class 酒 : BasicCard
    {
        public 酒()
        {
            type = "基本牌";
            name = "酒";
        }

        protected override async Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            if (dest.hp < 1) await new Recover(dest).Execute();
            else
            {
                dest.useJiu = true;
                dest.jiuCount++;
                TurnSystem.Instance.AfterPlay += () =>
                {
                    dest.useJiu = false;
                    dest.jiuCount = 0;
                };
            }
        }
    }
}
