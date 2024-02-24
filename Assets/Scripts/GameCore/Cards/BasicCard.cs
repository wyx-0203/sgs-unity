using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameCore
{
    public class BasicCard : Card
    {
        public BasicCard() : base() { }
    }

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
            game.turnSystem.AfterPlay += () => src.shaCount = 0;

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
            await Triggered.Invoke(game, x => x.OnEveryExecuteSha, this);

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

                var type = this is 火杀 ? Model.Damage.Type.Fire : this is 雷杀 ? Model.Damage.Type.Thunder : Model.Damage.Type.Normal;
                await new Damage(dest, src, this, 1 + damageOffset[dest.position], type).Execute();
            }
        }

        public bool needDoubleShan { get; set; }
        public bool ignoreArmor { get; set; }
        public bool isDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            // Timer.Instance.hint = "请打出一张杀。";
            // Timer.Instance.isValidCard = card => card.Useable<杀>();

            // var decision = await Timer.Instance.Run(player, 1, 0);
            var decision = await new PlayQuery
            {
                player = player,
                hint = "请打出一张杀。",
                isValidCard = card => card.Useable<杀>()
            }.Run(1, 0);
            if (!decision.action) return false;

            await decision.cards[0].Put(player);
            return true;
        }

        public override bool IsValid() => src.shaCount < 1 || src.effects.NoTimesLimit.Invoke(this);
        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => UseSha(src, player, this);

        public static bool UseSha(Player src, Player dest) => src != dest && src.attackRange >= src.GetDistance(dest);
        public static bool UseSha(Player src, Player dest, 杀 card) => src != dest
            && (src.attackRange >= src.GetDistance(dest) || src.effects.NoDistanceLimit.Invoke(card, dest));
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

            // Timer.Instance.hint = "请使用一张闪。";
            // Timer.Instance.isValidCard = card => card.Useable<闪>();
            // Timer.Instance.DefaultAI = AI.TryAction;

            // var decision = await Timer.Instance.Run(player, 1, 0);
            var decision = await new PlayQuery
            {
                player = player,
                hint = "请使用一张闪。",
                isValidCard = card => card.Useable<闪>(),
            }.Run(1, 0);

            if (!decision.action) return false;

            await decision.cards[0].UseCard(player);
            return true;
        }

        public override bool IsValid() => false;
    }

    public class 桃 : BasicCard
    {
        public 桃()
        {
            type = "基本牌";
            name = "桃";
        }

        protected override Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            return Task.CompletedTask;
        }

        protected override async Task UseForeachDest()
        {
            await new Recover(dest).Execute();
        }

        public static async Task<bool> Call(Player player, Player dest)
        {
            // Timer.Instance.hint = "请使用一张桃。";
            // Timer.Instance.isValidCard = card => (card is 桃 || card is 酒 && dest == player) && card.useable;
            // Timer.Instance.isValidDest = player => player == dest;
            // Timer.Instance.DefaultAI = player.team == dest.team ? AI.TryAction : () => new();

            // var decision = await Timer.Instance.Run(player, 1, 1);
            var decision = await new PlayQuery
            {
                player = player,
                hint = "请使用一张桃。",
                isValidCard = card => (card is 桃 || card is 酒 && dest == player) && card.useable,
                isValidDest = player => player == dest,
                aiAct = player.team == dest.team
            }.Run(1, 1);
            if (!decision.action) return false;

            await decision.cards[0].UseCard(player, new List<Player> { dest });
            return true;
        }

        public override bool IsValid() => src.hp < src.hpLimit;
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

        protected override Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            return Task.CompletedTask;
        }

        protected override async Task UseForeachDest()
        {
            if (dest.hp < 1) await new Recover(dest).Execute();
            else
            {
                dest.useJiu = true;
                dest.jiuCount++;
                game.turnSystem.AfterPlay += () =>
                {
                    dest.useJiu = false;
                    dest.jiuCount = 0;
                };
            }
        }
    }
}
