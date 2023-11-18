using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameCore
{
    public class Scheme : Card { }

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
            var card = await TimerAction.SelectOneCardFromElse(src, dest, true);

            if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                delayScheme.RemoveToJudgeArea();
                CardPile.Instance.AddToDiscard(card);
            }
            else await new Discard(dest, card).Execute();
        }
    }

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
            var card = await TimerAction.SelectOneCardFromElse(src, dest, true);

            if (card[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                await new GetJudgeCard(src, delayScheme).Execute();
            }
            else await new GetCardFromElse(src, dest, card).Execute();
        }
    }

    public class 决斗 : Scheme
    {
        public 决斗()
        {
            type = "锦囊牌";
            name = "决斗";
        }

        protected override async Task UseForeachDest()
        {
            src.FindSkill("无双")?.Execute();
            dest.FindSkill("无双")?.Execute();

            bool done = false;
            for (Player player = dest, other = src, t; !done; t = player, player = other, other = t)
            {
                done = !await 杀.Call(player);
                if (!done && other.FindSkill("无双") != null) done = !await 杀.Call(player);

                if (done) await new Damaged(player, other, this, 1 + damageOffset[player.position]).Execute();
            }
        }
    }

    public class 南蛮入侵 : Scheme
    {
        public 南蛮入侵()
        {
            type = "锦囊牌";
            name = "南蛮入侵";
        }

        protected override async Task BeforeUse()
        {
            dests = new List<Player>(Main.Instance.AlivePlayers);
            dests.Remove(src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            Timer.Instance.DefaultAI = AI.TryAction;
            if (!await 杀.Call(dest)) await new Damaged(dest, src, this).Execute();
        }
    }

    public class 万箭齐发 : Scheme
    {
        public 万箭齐发()
        {
            type = "锦囊牌";
            name = "万箭齐发";
        }

        protected override async Task BeforeUse()
        {
            dests = new List<Player>(Main.Instance.AlivePlayers);
            dests.Remove(src);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            Timer.Instance.DefaultAI = AI.TryAction;
            if (!await 闪.Call(dest, this)) await new Damaged(dest, src, this).Execute();
        }
    }

    public class 桃园结义 : Scheme
    {
        public 桃园结义()
        {
            type = "锦囊牌";
            name = "桃园结义";
        }

        protected override async Task BeforeUse()
        {
            dests = new List<Player>(Main.Instance.AlivePlayers);
            await Task.Yield();
        }

        protected override async Task AfterInit()
        {
            dests.RemoveAll(x => x.hp == x.hpLimit);
            await Task.Yield();
        }

        protected override async Task UseForeachDest()
        {
            await new Recover(dest).Execute();
        }
    }

    public class 无中生有 : Scheme
    {
        public 无中生有()
        {
            type = "锦囊牌";
            name = "无中生有";
        }

        protected override async Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
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

            var decision = await Timer.Instance.Run(src, 0, 1);
            ShaDest = decision.dests[0];

            Timer.Instance.isValidCard = card => card is 杀;
            Timer.Instance.isValidDest = dest => dest == ShaDest;
            Timer.Instance.DefaultAI = src.team != ShaDest.team ? AI.TryAction : () => new();

            decision = await Timer.Instance.Run(dest, 1, 1);
            // 出杀
            if (decision.action)
            {
                await decision.cards[0].UseCard(dest, decision.dests);
            }
            // 获得武器
            else await new GetCardFromElse(src, dest, new List<Card> { dest.weapon }).Execute();
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
            if (dests.Count > 0) return;

            Util.Print(src + "重铸了" + this);
            CardPile.Instance.AddToDiscard(this);
            await new LoseCard(src, new List<Card> { this }).Execute();
            await new GetCardFromPile(src, 1).Execute();
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
            if (dest.handCardsCount == 0) return;

            Timer.Instance.hint = src + "对你使用火攻，请展示一张手牌。";
            var showCard = (await TimerAction.ShowOneCard(dest))[0];

            Timer.Instance.hint = "是否弃置手牌";
            Timer.Instance.isValidCard = card => card.isHandCard && card.suit == showCard.suit;
            Timer.Instance.DefaultAI = () => src.team != dest.team || !AI.CertainValue ? AI.TryAction() : new Decision();
            var decision = await Timer.Instance.Run(src, 1, 0);

            if (!decision.action) return;
            await new Discard(src, decision.cards).Execute();
            await new Damaged(dest, src, this, 1, Damaged.Type.Fire).Execute();
        }
    }
}
