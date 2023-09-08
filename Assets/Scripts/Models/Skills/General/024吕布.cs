using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 无双 : Triggered
    {
        public override bool isObey => true;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i == Src) i.events.AfterUseCard.AddEvent(Src, ExecuteAsSrc);
                i.events.AfterUseCard.AddEvent(Src, ExecuteAsDest);
            }
        }

        public override void OnDisable()
        {
            Src.events.AfterUseCard.RemoveEvent(Src);
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.events.AfterUseCard.RemoveEvent(Src);
            }
        }

        public async Task ExecuteAsSrc(Card card)
        {
            if (card is 杀 sha) await Use杀(sha);
            else if (card is 决斗 juedou) await Use决斗(juedou);
        }

        public async Task ExecuteAsDest(Card card)
        {
            if (card is 决斗 juedou) await Use决斗(juedou);
        }

        public async Task Use杀(杀 card)
        {
            await Execute();
            foreach (var i in card.Dests)
            {
                if (card.ShanCount[i.position] == 1) card.ShanCount[i.position] = 2;
            }
        }

        public async Task Use决斗(决斗 card)
        {
            if (card.Src != Src && !card.Dests.Contains(Src)) return;

            await Execute();
            if (card.Src == Src) card.DestShaCount = 2;
            else card.SrcShaCount = 2;
        }
    }

    public class 利驭 : Triggered
    {
        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        private Player dest;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.AfterDamaged.AddEvent(Src, Execute);
            }
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.events.AfterDamaged.RemoveEvent(Src);
            }
        }

        public async Task Execute(Damaged damaged)
        {
            dest = damaged.player;

            // 触发条件
            if (damaged.Src != Src || damaged.SrcCard is not 杀) return;
            if (dest.CardCount == 0) return;

            var decision = await WaitDecision();
            if (!decision.action) return;
            await Execute(decision);

            CardPanel.Instance.Title = "利驭";
            CardPanel.Instance.Hint = "对" + dest.posStr + "号位发动利驭，获得其一张牌";
            var card = await TimerAction.SelectCard(Src, damaged.player);

            // 获得牌
            await new GetCardFromElse(Src, dest, card).Execute();

            // 若为装备牌
            if (card[0] is Equipment)
            {
                if (SgsMain.Instance.AlivePlayers.Count <= 2) return;

                // 指定角色
                Timer.Instance.hint = Src.posStr + "号位对你发动利驭，选择一名角色";
                Timer.Instance.isValidDest = player => player != Src && player != dest;
                Timer.Instance.refusable = false;
                Timer.Instance.AIDecision = AI.AutoDecision;
                decision = await Timer.Instance.Run(dest, 0, 1);

                // 使用决斗
                await Card.Convert<决斗>(new List<Card>()).UseCard(Src, decision.dests);
            }
            // 摸牌
            else await new GetCardFromPile(dest, 1).Execute();
        }
    }
}
