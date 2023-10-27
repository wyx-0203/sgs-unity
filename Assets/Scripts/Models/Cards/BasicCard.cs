using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class 杀 : Card
    {
        /// <summary>
        /// 杀
        /// </summary>
        public 杀()
        {
            type = "基本牌";
            name = "杀";
        }

        protected override async Task BeforeUse()
        {
            shanCount = 1;
            IgnoreArmor = false;
            await Task.Yield();
        }

        protected override async Task AfterInit()
        {
            // 青釭剑 雌雄双股剑
            if (Src.weapon != null) await Src.weapon.BeforeUseSha(this);

            if (Src.useJiu)
            {
                Src.useJiu = false;
                for (int i = 0; i < damageOffset.Length; i++) damageOffset[i]++;
            }
        }

        protected override async Task UseForeachDest()
        {
            IsDamage = false;
            if (shanCount == 0 || Src.effects.Unmissable.Invoke(this, dest)) IsDamage = true;
            else
            {
                for (int i = 0; i < shanCount; i++)
                {
                    // Timer.Instance.AIDecision = AI.AutoDecision;
                    if (!await 闪.Call(dest, this))
                    {
                        IsDamage = true;
                        break;
                    }
                }
            }

            if (!IsDamage && Src.weapon != null) await Src.weapon.OnShaMissed(this);

            if (IsDamage)
            {
                try { if (Src.weapon != null) await Src.weapon.OnShaDamage(this); }
                catch (PreventDamage) { return; }

                Damaged.Type type = this is 火杀 ? Damaged.Type.Fire : this is 雷杀 ? Damaged.Type.Thunder : Damaged.Type.Normal;
                await new Damaged(dest, Src, this, 1 + damageOffset[dest.position], type).Execute();
            }
        }

        public int shanCount { get; set; }
        public bool IgnoreArmor { get; set; }
        public bool IsDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            Timer.Instance.hint = "请打出一张杀。";
            Timer.Instance.isValidCard = card => card is 杀 && card.useable;

            var decision = await Timer.Instance.Run(player, 1, 0);
            if (!decision.action) return false;

            await decision.cards[0].Put(player);
            return true;
        }
    }

    /// <summary>
    /// 闪
    /// </summary>
    public class 闪 : Card
    {
        public 闪()
        {
            type = "基本牌";
            name = "闪";
        }

        public static async Task<bool> Call(Player player, 杀 sha = null)
        {
            if (player.armor is 八卦阵 baguazhen && (sha is null || !sha.IgnoreArmor))
            {
                bool result = await baguazhen.Skill();
                if (result) return true;
            }

            Timer.Instance.hint = "请使用一张闪。";
            Timer.Instance.isValidCard = card => card is 闪 && card.useable;
            Timer.Instance.DefaultAI = AI.TryAction;

            var decision = await Timer.Instance.Run(player, 1, 0);

            if (!decision.action) return false;

            await decision.cards[0].UseCard(player);
            return true;
        }
    }

    /// <summary>
    /// 桃
    /// </summary>
    public class 桃 : Card
    {
        public 桃()
        {
            type = "基本牌";
            name = "桃";
        }

        protected override async Task BeforeUse()
        {
            if (Dests.Count == 0) Dests.Add(Src);
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

    public class 酒 : Card
    {
        public 酒()
        {
            type = "基本牌";
            name = "酒";
        }

        protected override async Task BeforeUse()
        {
            if (Dests.Count == 0) Dests.Add(Src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            if (dest.Hp < 1) await new Recover(dest).Execute();
            else
            {
                dest.useJiu = true;
                dest.jiuCount++;
            }
        }
    }
}
