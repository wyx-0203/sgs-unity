using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class Scheme : Card
    {
        // protected new async Task<bool> UseForeachDest(Player dest)
        // {
        //     return base.UseForeachDest(dest) && !await 无懈可击.Call(this);
        // }
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

        protected override async Task AfterInit()
        {
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

        protected override async Task UseForeachDest()
        {
            CardPanel.Instance.Title = "过河拆桥";
            CardPanel.Instance.Hint = "对" + dest + "使用过河拆桥，弃置其一张牌";
            var card = await TimerAction.SelectOneCard(Src, dest, true);

            if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                delayScheme.RemoveToJudgeArea();
                CardPile.Instance.AddToDiscard(card);
            }
            else await new Discard(dest, card).Execute();
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

        protected override async Task UseForeachDest()
        {
            CardPanel.Instance.Title = "顺手牵羊";
            CardPanel.Instance.Hint = "对" + dest + "使用顺手牵羊，获得其一张牌";
            var card = await TimerAction.SelectOneCard(Src, dest, true);

            if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                await new GetJudgeCard(Src, delayScheme).Execute();
            }
            else await new GetCardFromElse(Src, dest, card).Execute();
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

        protected override async Task UseForeachDest()
        {
            var player = dest;
            bool done = false;
            int shaCount = 0;
            SrcShaCount = 1;
            DestShaCount = 1;

            while (!done)
            {

                Timer.Instance.DefaultAI = Src.team != dest.team ? AI.TryAction : () => new();
                done = !await 杀.Call(player);

                if (done) await new Damaged(player, player == dest ? Src : dest, this).Execute();
                else
                {
                    shaCount++;

                    if (shaCount >= (player == Src ? SrcShaCount : DestShaCount))
                    {
                        player = player == dest ? Src : dest;
                        shaCount = 0;
                    }
                }
            }
        }

        public int SrcShaCount { get; set; }
        public int DestShaCount { get; set; }
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

        protected override async Task BeforeUse()
        {
            Dests = new List<Player>(SgsMain.Instance.AlivePlayers);
            Dests.Remove(Src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            Timer.Instance.DefaultAI = AI.TryAction;
            if (!await 杀.Call(dest)) await new Damaged(dest, Src, this).Execute();
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

        protected override async Task BeforeUse()
        {
            Dests = new List<Player>(SgsMain.Instance.AlivePlayers);
            Dests.Remove(Src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            Timer.Instance.DefaultAI = AI.TryAction;
            if (!await 闪.Call(dest)) await new Damaged(dest, Src, this).Execute();
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

        protected override async Task BeforeUse()
        {
            Dests = new List<Player>(SgsMain.Instance.AlivePlayers);
            await Task.Yield();
        }

        protected override async Task AfterInit()
        {
            Dests.RemoveAll(x => x.Hp == x.HpLimit);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            await new Recover(dest).Execute();
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

        protected override async Task BeforeUse()
        {
            if (Dests.Count == 0) Dests.Add(Src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            await new GetCardFromPile(dest, 2).Execute();
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

        protected override async Task UseForeachDest()
        {
            Timer.Instance.hint = "指定被杀的角色";
            Timer.Instance.isValidDest = x => DestArea.Instance.UseSha(dest, x);
            Timer.Instance.refusable = false;
            Timer.Instance.DefaultAI = AI.TryAction;

            var decision = await Timer.Instance.Run(Src, 0, 1);
            ShaDest = decision.dests[0];


            Timer.Instance.isValidCard = card => card is 杀;
            Timer.Instance.isValidDest = dest => dest == ShaDest;
            Timer.Instance.DefaultAI = Src.team != ShaDest.team ? AI.TryAction : () => new();

            decision = await Timer.Instance.Run(dest, 1, 1);
            // 出杀
            if (decision.action)
            {
                await decision.cards[0].UseCard(dest, decision.dests);
            }
            // 获得武器
            else await new GetCardFromElse(Src, dest, new List<Card> { dest.weapon }).Execute();
        }
    }

    public class 铁索连环 : Scheme
    {
        public 铁索连环()
        {
            type = "锦囊牌";
            name = "铁索连环";
        }

        protected override async Task BeforeUse()
        {
            if (Dests.Count > 0) return;

            Util.Print(Src + "重铸了" + this);
            CardPile.Instance.AddToDiscard(this);
            await new LoseCard(Src, new List<Card> { this }).Execute();
            await new GetCardFromPile(Src, 1).Execute();
            throw new CancelUseCard();
        }

        protected override async Task UseForeachDest()
        {
            await new SetLock(dest).Execute();
        }
    }

    public class 火攻 : Scheme
    {
        public 火攻()
        {
            type = "锦囊牌";
            name = "火攻";
        }

        protected override async Task UseForeachDest()
        {
            if (dest.HandCardCount == 0) return;

            Timer.Instance.hint = Src + "对你使用火攻，请展示一张手牌。";
            var showCard = (await TimerAction.ShowOneCard(dest))[0];

            Timer.Instance.hint = "是否弃置手牌";
            Timer.Instance.isValidCard = card => card.isHandCard && card.suit == showCard.suit;
            Timer.Instance.DefaultAI = () => Src.team != dest.team || !AI.CertainValue ? AI.TryAction() : new Decision();
            var decision = await Timer.Instance.Run(Src, 1, 0);

            if (!decision.action) return;
            await new Discard(Src, decision.cards).Execute();
            await new Damaged(dest, Src, this, 1, Damaged.Type.Fire).Execute();
        }
    }
}
