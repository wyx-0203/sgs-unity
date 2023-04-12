using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 无懈可击
    /// </summary>
    public class 无懈可击 : Card
    {
        public 无懈可击()
        {
            Type = "锦囊牌";
            Name = "无懈可击";
        }

        public static async Task<bool> Call(Card card, Player dest)
        {
            string hint = dest != null ? "对" + dest.posStr + "号位" : "";
            Timer.Instance.Hint = card.Name + "即将" + hint + "生效，是否使用无懈可击？";

            bool result = await Timer.Instance.RunWxkj();
            if (result)
            {
                Debug.Log(Timer.Instance.Cards[0].Name);
                var wxkj = Timer.Instance.Cards[0] as 无懈可击;
                await wxkj.UseCard(Timer.Instance.player);
                if (!wxkj.isCountered) return true;
            }
            return false;
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);
            isCountered = await Call(this, null);
        }

        private bool isCountered;
    }

    /// <summary>
    /// 过河拆桥
    /// </summary>
    public class 过河拆桥 : Card
    {
        public 过河拆桥()
        {
            Type = "锦囊牌";
            Name = "过河拆桥";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                CardPanel.Instance.Title = "过河拆桥";
                CardPanel.Instance.Hint = "对" + dest.posStr + "号位使用过河拆桥，弃置其一张牌";
                var card = await CardPanel.Instance.SelectCard(src, dest, true);

                if (card is DelayScheme && dest.JudgeArea.Contains(card as DelayScheme))
                {
                    (card as DelayScheme).RemoveToJudgeArea();
                    CardPile.Instance.AddToDiscard(card);
                }
                else await new Discard(dest, new List<Card> { card }).Execute();
            }
        }
    }

    /// <summary>
    /// 顺手牵羊
    /// </summary>
    public class 顺手牵羊 : Card
    {
        public 顺手牵羊()
        {
            Type = "锦囊牌";
            Name = "顺手牵羊";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                CardPanel.Instance.Title = "顺手牵羊";
                CardPanel.Instance.Hint = "对" + dest.posStr + "号位使用顺手牵羊，获得其一张牌";
                var card = await CardPanel.Instance.SelectCard(src, dest, true);

                if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
                {
                    // ((DelayScheme)card).RemoveToJudgeArea();
                    await new GetJudgeCard(src, card).Execute();
                }
                else await new GetCardFromElse(src, dest, new List<Card> { card }).Execute();
            }
        }
    }

    /// <summary>
    /// 决斗
    /// </summary>
    public class 决斗 : Card
    {
        public 决斗()
        {
            Type = "锦囊牌";
            Name = "决斗";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                var player = dest;
                bool done = false;
                int shaCount = 0;
                while (!done)
                {
                    done = !await 杀.Call(player);

                    if (done) await new Damaged(player, player == dest ? src : dest, this).Execute();
                    else
                    {
                        shaCount++;

                        if (shaCount >= (player == src ? SrcShaCount : DestShaCount))
                        {
                            player = player == dest ? src : dest;
                            shaCount = 0;
                        }
                    }
                }
            }
        }

        public int SrcShaCount { get; set; } = 1;
        public int DestShaCount { get; set; } = 1;
    }

    /// <summary>
    /// 南蛮入侵
    /// </summary>
    public class 南蛮入侵 : Card
    {
        public 南蛮入侵()
        {
            Type = "锦囊牌";
            Name = "南蛮入侵";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                if (!await 杀.Call(dest)) await new Damaged(dest, src, this).Execute();
            }
        }
    }

    /// <summary>
    /// 万箭齐发
    /// </summary>
    public class 万箭齐发 : Card
    {
        public 万箭齐发()
        {
            Type = "锦囊牌";
            Name = "万箭齐发";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                if (!await 闪.Call(dest)) await new Damaged(dest, src, this).Execute();
            }
        }
    }

    /// <summary>
    /// 桃园结义
    /// </summary>
    public class 桃园结义 : Card
    {
        public 桃园结义()
        {
            Type = "锦囊牌";
            Name = "桃园结义";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (dest.Hp >= dest.HpLimit) continue;
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                await new Recover(dest).Execute();
            }
        }
    }

    /// <summary>
    /// 无中生有
    /// </summary>
    public class 无中生有 : Card
    {
        public 无中生有()
        {
            Type = "锦囊牌";
            Name = "无中生有";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;

                await new GetCardFromPile(dest, 2).Execute();
            }
        }
    }

    public class 借刀杀人 : Card
    {
        public 借刀杀人()
        {
            Type = "锦囊牌";
            Name = "借刀杀人";
        }

        public Player ShaDest { get; private set; }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            ShaDest = dests[1];
            dests.RemoveAt(1);
            await base.UseCard(src, dests);

            if (Disabled(dests[0])) return;
            if (await 无懈可击.Call(this, dests[0])) return;

            Timer.Instance.IsValidCard = card => card is 杀;
            Timer.Instance.IsValidDest = dest => dest == ShaDest;

            var result = await Timer.Instance.Run(dests[0], 1, 1);
            // 出杀
            if (result)
            {
                await Timer.Instance.Cards[0].UseCard(dests[0], Timer.Instance.Dests);
            }
            // 获得武器
            else await new GetCardFromElse(Src, dests[0], new List<Card> { dests[0].weapon }).Execute();
        }
    }

    public class 铁索连环 : Card
    {
        public 铁索连环()
        {
            Type = "锦囊牌";
            Name = "铁索连环";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            if (dests is null || dests.Count == 0)
            {
                CardPile.Instance.AddToDiscard(this);
                await new LoseCard(src, new List<Card> { this }).Execute();
                await new GetCardFromPile(src, 1).Execute();
                return;
            }

            await base.UseCard(src, dests);

            foreach (var i in Dests)
            {
                if (Disabled(i)) continue;
                if (await 无懈可击.Call(this, i)) continue;

                await new SetLock(i).Execute();
            }
        }
    }

    public class 火攻 : Card
    {
        public 火攻()
        {
            Type = "锦囊牌";
            Name = "火攻";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (Disabled(dest)) continue;
                if (await 无懈可击.Call(this, dest)) continue;
                if (dest.HandCardCount == 0) continue;

                Timer.Instance.Hint = Src.posStr + "号位对你使用火攻，请展示一张手牌。";
                var showCard = (await TimerAction.ShowCardTimer(dest))[0];

                Timer.Instance.Hint = "是否弃置手牌";
                Timer.Instance.IsValidCard = card => card.Suit == showCard.Suit;
                if (!await Timer.Instance.Run(Src, 1, 0)) return;

                await new Discard(Src, Timer.Instance.Cards).Execute();
                await new Damaged(dest, Src, this, 1, DamageType.Fire).Execute();
            }
        }
    }
}
