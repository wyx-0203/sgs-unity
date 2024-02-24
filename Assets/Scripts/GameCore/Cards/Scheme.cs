using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team = Model.Team;

namespace GameCore
{
    public class Scheme : Card { public Scheme() { } }

    public class 无懈可击 : Scheme
    {
        public 无懈可击()
        {
            type = "锦囊牌";
            name = "无懈可击";
        }

        private bool isCountered;

        public static async Task<bool> Call(Card scheme)
        {
            var team = scheme.game.turnSystem.CurrentPlayer.team;
            var decision = await WaitPlay(scheme, team);
            // var decision = await Timer.Instance.Run
            if (!decision.action) decision = await WaitPlay(scheme, ~team);

            if (decision.action)
            {
                var wxkj = decision.cards[0] as 无懈可击;
                await wxkj.UseCard(decision.src);
                if (!wxkj.isCountered) return true;
            }
            return false;
        }

        private static async Task<PlayDecision> WaitPlay(Card scheme, Team team) => await new PlayQuery
        {
            player = scheme.game.AlivePlayers.Find(x => x.team == team),
            hint = $"{scheme}即将对{scheme.dest}生效，是否使用无懈可击？",
            isValidCard = x => x is 无懈可击,
            type = Model.SinglePlayQuery.Type.WXKJ,
            aiAct = scheme.src.team != team ^ scheme is DelayScheme,
            defaultAI = () =>
            {
                foreach (var i in scheme.game.AlivePlayers.Where(x => x.team == team))
                {
                    var card = i.FindCard<无懈可击>();
                    if (card is null) continue;

                    return new PlayDecision { src = i, cards = new List<Card> { card } };
                }
                // var player=game.AlivePlayers.Find(x=>x.team==team&&x.FindCard<无懈可击>() is Card card1);
                // return player!=null?new PlayDecision{action=true,cards=new List<Card>{card1}}
                return new();
            }
        }.Run(1, 0);

        protected override async Task AfterInit()
        {
            // if (!MCTS.Instance.isRunning) 
            // await new Delay(0.2f).Run();
            await Delay.Run(200);
            isCountered = await Call(this);
        }

        public override bool IsValid() => false;
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
            // CardPanelRequest.Instance.title = "过河拆桥";
            string hint = $"对{dest}使用过河拆桥，弃置其一张牌";
            var cards = await new CardPanelQuery(src, dest, name, hint, true).Run();
            // var card = await TimerAction.SelectCardFromElse(src, dest, true);

            if (cards[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                delayScheme.RemoveToJudgeArea();
                game.cardPile.AddToDiscard(cards, dest);
            }
            else await new Discard(dest, cards).Execute();
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => src != player && !player.regionIsEmpty;
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
            // CardPanelRequest.Instance.title = "顺手牵羊";
            string hint = hint = $"对{dest}使用顺手牵羊，获得其一张牌";
            // var cards = await TimerAction.SelectCardFromElse(src, dest, true);
            var cards = await new CardPanelQuery(src, dest, name, hint, true).Run();

            if (cards[0] is DelayScheme delayScheme && dest.JudgeCards.Contains(delayScheme))
            {
                await new GetJudgeCard(src, delayScheme).Execute();
            }
            else await new GetAnothersCard(src, dest, cards).Execute();
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => src.GetDistance(player) == 1 && !player.regionIsEmpty;
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

                if (done) await new Damage(player, other, this, 1 + damageOffset[player.position]).Execute();
            }
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => player != src;
    }

    public class 南蛮入侵 : Scheme
    {
        public 南蛮入侵()
        {
            type = "锦囊牌";
            name = "南蛮入侵";
        }

        protected override Task BeforeUse()
        {
            dests = new List<Player>(game.AlivePlayers);
            dests.Remove(src);
            return Task.CompletedTask;
        }

        protected override async Task UseForeachDest()
        {
            // Timer.Instance.defaultAI = AI.TryAction;
            if (!await 杀.Call(dest)) await new Damage(dest, src, this).Execute();
        }
    }

    public class 万箭齐发 : Scheme
    {
        public 万箭齐发()
        {
            type = "锦囊牌";
            name = "万箭齐发";
        }

        protected override Task BeforeUse()
        {
            dests = new List<Player>(game.AlivePlayers);
            dests.Remove(src);
            return Task.CompletedTask;
        }

        protected override async Task UseForeachDest()
        {
            // Timer.Instance.defaultAI = AI.TryAction;
            if (!await 闪.Call(dest, this)) await new Damage(dest, src, this).Execute();
        }
    }

    public class 桃园结义 : Scheme
    {
        public 桃园结义()
        {
            type = "锦囊牌";
            name = "桃园结义";
        }

        protected override Task BeforeUse()
        {
            dests = new List<Player>(game.AlivePlayers);
            return Task.CompletedTask;
        }

        protected override Task AfterInit()
        {
            dests.RemoveAll(x => x.hp == x.hpLimit);
            return Task.CompletedTask;
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

        protected override Task BeforeUse()
        {
            if (dests.Count == 0) dests.Add(src);
            return Task.CompletedTask;
        }

        protected override async Task UseForeachDest()
        {
            await new DrawCard(dest, 2).Execute();
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
            // Timer.Instance.hint = "指定被杀的角色";
            // Timer.Instance.isValidDest = x => DestArea.Instance.UseSha(dest, x);
            // Timer.Instance.refusable = false;
            // Timer.Instance.DefaultAI = AI.TryAction;

            // var decision = await Timer.Instance.Run(src, 0, 1);
            var decision = await new PlayQuery
            {
                player = src,
                hint = "指定被杀的角色",
                isValidDest = player => 杀.UseSha(dest, player),
                refusable = false,
            }.Run(0, 1);
            ShaDest = decision.dests[0];

            // Timer.Instance.isValidCard = card => card is 杀;
            // Timer.Instance.isValidDest = dest => dest == ShaDest;
            // Timer.Instance.DefaultAI = src.team != ShaDest.team ? AI.TryAction : () => new();

            // decision = await Timer.Instance.Run(dest, 1, 1);
            decision = await new PlayQuery
            {
                isValidCard = card => card is 杀,
                isValidDest = dest => dest == ShaDest,
                aiAct = src.team != ShaDest.team
            }.Run(1, 1);
            // 出杀
            if (decision.action)
            {
                await decision.cards[0].UseCard(dest, decision.dests);
            }
            // 获得武器
            else await new GetAnothersCard(src, dest, new List<Card> { dest.weapon }).Execute();
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => player != src
            && player.weapon != null
            && game.AlivePlayers.Find(x => 杀.UseSha(player, x)) != null;
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

            // Util.Print(src + "重铸了" + this);
            game.eventSystem.SendToClient(new Model.Message { text = $"{src}重铸了{this}" });
            game.cardPile.AddToDiscard(this, src);
            await new LoseCard(src, new List<Card> { this }).Execute();
            await new DrawCard(src, 1).Execute();
            throw new CancelUseCard();
        }

        protected override async Task UseForeachDest()
        {
            await new SetLock(dest).Execute();
        }

        public override int MaxDest() => 2;
        public override bool IsValidDest(Player player) => true;
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

            // Timer.Instance.hint = ;
            var showCard = (await TimerAction.ShowOneCard(dest, $"{src}对你使用火攻，请展示一张手牌。"))[0];

            // Timer.Instance.hint = "是否弃置手牌";
            // Timer.Instance.isValidCard = card => card.isHandCard && card.suit == showCard.suit;
            // Timer.Instance.DefaultAI = () => src.team != dest.team || !AI.CertainValue ? AI.TryAction() : new Decision();
            // var decision = await Timer.Instance.Run(src, 1, 0);
            var decision = await new PlayQuery
            {
                player = src,
                isValidCard = card => card.isHandCard && card.suit == showCard.suit,
                // defaultAI = () => src.team != dest.team || !AI.CertainValue ? AI.TryAction() : new Decision(),
                aiAct = src.team != dest.team
            }.Run(1, 0);

            if (!decision.action) return;
            await new Discard(src, decision.cards).Execute();
            await new Damage(dest, src, this, 1, Model.Damage.Type.Fire).Execute();
        }

        public override int MaxDest() => 1;
        public override int MinDest() => 1;
        public override bool IsValidDest(Player player) => player.handCardsCount != 0;
    }
}
