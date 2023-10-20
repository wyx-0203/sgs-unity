using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class Scheme : Card
    {
        protected new async Task<bool> UseForeachDest(Player dest)
        {
            return base.UseForeachDest(dest) && !await 无懈可击.Call(this);
        }
    }
    /// <summary>
    /// 无懈可击
    /// </summary>
    public class 无懈可击 : Scheme
    {
        public 无懈可击()
        {
            type = "锦囊牌";
            name = "无懈可击";
        }

        private bool isCountered;

        public static async Task<bool> Call(Card card)
        {
            var team = TurnSystem.Instance.CurrentPlayer.team;
            var decision = await Timer.Instance.RunWxkj(card, team);
            if (!decision.action) decision = await Timer.Instance.RunWxkj(card, !team);

            if (decision.action)
            {
                var wxkj = decision.cards[0] as 无懈可击;
                await wxkj.UseCard(decision.src);
                if (!wxkj.isCountered) return true;
            }
            return false;
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);
            if (!MCTS.Instance.isRunning) await new Delay(0.2f).Run();
            isCountered = await Call(this);
        }
    }

    /// <summary>
    /// 过河拆桥
    /// </summary>
    public class 过河拆桥 : Scheme
    {
        public 过河拆桥()
        {
            type = "锦囊牌";
            name = "过河拆桥";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                CardPanel.Instance.Title = "过河拆桥";
                CardPanel.Instance.Hint = "对" + dest.posStr + "号位使用过河拆桥，弃置其一张牌";
                var card = await TimerAction.SelectOneCard(src, dest, true);

                if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
                {
                    delayScheme.RemoveToJudgeArea();
                    CardPile.Instance.AddToDiscard(card);
                }
                else await new Discard(dest, card).Execute();
            }
        }
    }

    /// <summary>
    /// 顺手牵羊
    /// </summary>
    public class 顺手牵羊 : Scheme
    {
        public 顺手牵羊()
        {
            type = "锦囊牌";
            name = "顺手牵羊";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                CardPanel.Instance.Title = "顺手牵羊";
                CardPanel.Instance.Hint = "对" + dest.posStr + "号位使用顺手牵羊，获得其一张牌";
                var card = await TimerAction.SelectOneCard(src, dest, true);

                if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
                {
                    // ((DelayScheme)card).RemoveToJudgeArea();
                    await new GetJudgeCard(src, delayScheme).Execute();
                }
                else await new GetCardFromElse(src, dest, card).Execute();
            }
        }
    }

    /// <summary>
    /// 决斗
    /// </summary>
    public class 决斗 : Scheme
    {
        public 决斗()
        {
            type = "锦囊牌";
            name = "决斗";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                var player = dest;
                bool done = false;
                int shaCount = 0;
                while (!done)
                {

                    Timer.Instance.DefaultAI = src.team != dest.team ? AI.TryAction : () => new();
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
    public class 南蛮入侵 : Scheme
    {
        public 南蛮入侵()
        {
            type = "锦囊牌";
            name = "南蛮入侵";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                Timer.Instance.DefaultAI = AI.TryAction;
                if (!await 杀.Call(dest)) await new Damaged(dest, src, this).Execute();
            }
        }
    }

    /// <summary>
    /// 万箭齐发
    /// </summary>
    public class 万箭齐发 : Scheme
    {
        public 万箭齐发()
        {
            type = "锦囊牌";
            name = "万箭齐发";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                // Timer.Instance.AIDecision = AI.AutoDecision;
                if (!await 闪.Call(dest)) await new Damaged(dest, src, this).Execute();
            }
        }
    }

    /// <summary>
    /// 桃园结义
    /// </summary>
    public class 桃园结义 : Scheme
    {
        public 桃园结义()
        {
            type = "锦囊牌";
            name = "桃园结义";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (dest.Hp >= dest.HpLimit) continue;
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                await new Recover(dest).Execute();
            }
        }
    }

    /// <summary>
    /// 无中生有
    /// </summary>
    public class 无中生有 : Scheme
    {
        public 无中生有()
        {
            type = "锦囊牌";
            name = "无中生有";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;

                await new GetCardFromPile(dest, 2).Execute();
            }
        }
    }

    public class 借刀杀人 : Scheme
    {
        public 借刀杀人()
        {
            type = "锦囊牌";
            name = "借刀杀人";
        }

        public Player ShaDest { get; private set; }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);

            foreach (var i in dests)
            {
                // if (Disabled(i)) continue;
                if (!await UseForeachDest(i)) continue;
                // if (await 无懈可击.Call(this, i)) continue;

                Timer.Instance.hint = "指定被杀的角色";
                Timer.Instance.isValidDest = x => DestArea.Instance.UseSha(i, x);
                Timer.Instance.refusable = false;
                Timer.Instance.DefaultAI = AI.TryAction;

                var decision = await Timer.Instance.Run(src, 0, 1);
                ShaDest = decision.dests[0];


                Timer.Instance.isValidCard = card => card is 杀;
                Timer.Instance.isValidDest = dest => dest == ShaDest;
                Timer.Instance.DefaultAI = src.team != ShaDest.team ? AI.TryAction : () => new();

                decision = await Timer.Instance.Run(i, 1, 1);
                // 出杀
                if (decision.action)
                {
                    await decision.cards[0].UseCard(i, decision.dests);
                }
                // 获得武器
                else await new GetCardFromElse(Src, i, new List<Card> { i.weapon }).Execute();
            }
        }
    }

    public class 铁索连环 : Scheme
    {
        public 铁索连环()
        {
            type = "锦囊牌";
            name = "铁索连环";
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
                // if (Disabled(i)) continue;
                if (!await UseForeachDest(i)) continue;
                // if (await 无懈可击.Call(this, i)) continue;

                await new SetLock(i).Execute();
            }
        }
    }

    public class 火攻 : Scheme
    {
        public 火攻()
        {
            type = "锦囊牌";
            name = "火攻";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                // if (Disabled(dest)) continue;
                if (!await UseForeachDest(dest)) continue;
                // if (await 无懈可击.Call(this, dest)) continue;
                if (dest.HandCardCount == 0) continue;

                Timer.Instance.hint = Src.posStr + "号位对你使用火攻，请展示一张手牌。";
                var showCard = (await TimerAction.ShowOneCard(dest))[0];

                Timer.Instance.hint = "是否弃置手牌";
                Timer.Instance.isValidCard = card => card.IsHandCard && card.suit == showCard.suit;
                Timer.Instance.DefaultAI = () => src.team != dest.team || !AI.CertainValue ? AI.TryAction() : new Decision();
                var decision = await Timer.Instance.Run(Src, 1, 0);

                if (!decision.action) return;
                await new Discard(Src, decision.cards).Execute();
                await new Damaged(dest, Src, this, 1, DamageType.Fire).Execute();
            }
        }
    }
}
