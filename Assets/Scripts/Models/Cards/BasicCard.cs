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

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            if (src.weapon != null)
            {
                Src = src;
                Dests = dests;
                if (src.weapon is 朱雀羽扇 zqys && await zqys.Skill(this)) return;
                src.weapon.WhenUseSha(this);
            }

            for (int i = 0; i < ShanCount.Length; i++) ShanCount[i] = 1;
            for (int i = 0; i < DamageValue.Length; i++) DamageValue[i] = 1;
            IgnoreArmor = false;

            await base.UseCard(src, dests);

            if (src.Use酒)
            {
                src.Use酒 = false;
                for (int i = 0; i < DamageValue.Length; i++) DamageValue[i]++;
            }

            // 青釭剑 雌雄双股剑
            if (src.weapon != null) await src.weapon.AfterUseSha(this);

            foreach (var dest in Dests)
            {
                // 仁王盾 藤甲
                // if (Disabled(dest)) continue;

                IsDamage = false;
                if (ShanCount[dest.position] == 0) IsDamage = true;
                else
                {
                    for (int i = 0; i < ShanCount[dest.position]; i++)
                    {
                        // Timer.Instance.AIDecision = AI.AutoDecision;
                        if (!await 闪.Call(dest, this))
                        {
                            IsDamage = true;
                            break;
                        }
                    }
                }

                if (!IsDamage && src.weapon != null) await src.weapon.ShaMiss(this, dest);

                if (IsDamage)
                {
                    if (src.weapon != null) await src.weapon.WhenDamage(this, dest);
                    if (!IsDamage) continue;
                    DamageType type = this is 火杀 ? DamageType.Fire : this is 雷杀 ? DamageType.Thunder : DamageType.Normal;
                    await new Damaged(dest, Src, this, DamageValue[dest.position], type).Execute();
                }
            }
        }

        public int[] ShanCount { get; set; } = new int[4];
        public int[] DamageValue { get; set; } = new int[4];
        public bool IgnoreArmor { get; set; }
        public bool IsDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            Timer.Instance.hint = "请打出一张杀。";
            Timer.Instance.isValidCard = card => card is 杀 && !player.DisabledCard(card);

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
            if (player.Equipments["防具"] is 八卦阵 baguazhen && (sha is null || !sha.IgnoreArmor))
            {
                bool result = await baguazhen.Skill();
                if (result) return true;
            }

            Timer.Instance.hint = "请使用一张闪。";
            Timer.Instance.isValidCard = card => card is 闪 && !player.DisabledCard(card);
            Timer.Instance.AIDecision = AI.AutoDecision;

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

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            // 回复体力
            foreach (var dest in Dests) await new Recover(dest).Execute();
        }

        public static async Task<bool> Call(Player player, Player dest)
        {
            Timer.Instance.hint = "请使用一张桃。";
            Timer.Instance.isValidCard = card => (card is 桃 || card is 酒 && dest == player)
                && !player.DisabledCard(card);
            Timer.Instance.isValidDest = player => player == dest;
            Timer.Instance.AIDecision = () =>
            {
                Card card = null;
                if (player == dest) card = player.FindCard<酒>();
                if (card is null && player.team == dest.team) card = player.FindCard<桃>();

                if (card is null || Random.value < 0.1f) return new Decision();
                return new Decision { cards = new List<Card> { card } };
            };
            var decision = await Timer.Instance.Run(player, 1, 1);

            // if (player.isAI && (player == dest || player.team == dest.team))
            // {
            //     var card = player.FindCard<酒>() as Card;
            //     if (card is null) card = player.FindCard<桃>();
            //     if (card != null)
            //     {
            //         Timer.Instance.cards = new List<Card> { card };
            //         // Timer.Instance.Cards.Add(card);
            //         result = true;
            //     }
            // }

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

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            if (Dests[0].Hp < 1) await new Recover(Dests[0]).Execute();
            else
            {
                Dests[0].Use酒 = true;
                Dests[0].酒Count++;
            }
        }
    }
}
