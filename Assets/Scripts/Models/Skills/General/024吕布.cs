using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class 无双 : Triggered
    {
        public 无双(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            // Src.playerEvents.afterUseCard.AddEvent(Src, Use杀);
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i == Src) i.events.AfterUseCard.AddEvent(Src, SelfExecute);
                i.events.AfterUseCard.AddEvent(Src, ElseExecute);
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

        public async Task SelfExecute(Card card)
        {
            if (card is 杀) await Use杀(card as 杀);
            else if (card is 决斗) await Use决斗(card as 决斗);
        }

        public async Task ElseExecute(Card card)
        {
            if (card is 决斗) await Use决斗(card as 决斗);
        }

        public async Task Use杀(杀 card)
        {
            // if (card is not 杀) return;
            // if ((card as 杀).ShanCount != 1) return;

            await Task.Yield();
            Execute();
            foreach (var i in card.Dests)
            {
                if (card.ShanCount[i.position] == 1) card.ShanCount[i.position] = 2;
            }
            // (card as 杀).ShanCount = 2;
        }

        public async Task Use决斗(决斗 card)
        {
            // if (card is not 决斗) return;
            if (card.Src != Src && !card.Dests.Contains(Src)) return;

            await Task.Yield();
            Execute();
            if (card.Src == Src) card.DestShaCount = 2;
            else card.SrcShaCount = 2;
        }
    }

    public class 利驭 : Triggered
    {
        public 利驭(Player src) : base(src) { }

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
            if (damaged.Src != Src || !(damaged.SrcCard is 杀)) return;
            if (dest.CardCount == 0) return;

            if (!await base.ShowTimer()) return;
            Execute();

            CardPanel.Instance.Title = "利驭";
            CardPanel.Instance.Hint = "对" + dest.posStr + "号位发动利驭，获得其一张牌";
            var card = await CardPanel.Instance.SelectCard(Src, damaged.player);

            // 获得牌
            await new GetCardFromElse(Src, dest, new List<Card> { card }).Execute();

            // 若为装备牌
            if (card is Equipage)
            {
                if (SgsMain.Instance.AlivePlayers.Count <= 2) return;

                // 指定角色
                Timer.Instance.Hint = Src.posStr + "号位对你发动利驭，选择一名角色";
                Timer.Instance.IsValidDest = player => player != Src && player != dest;
                bool result = await Timer.Instance.Run(dest, 0, 1);

                Player dest1 = null;
                if (!result)
                {
                    foreach (var i in SgsMain.Instance.AlivePlayers)
                    {
                        if (i != Src && i != dest)
                        {
                            dest1 = i;
                            break;
                        }
                    }
                }
                else dest1 = Timer.Instance.Dests[0];

                // 使用决斗
                await Card.Convert<决斗>(new List<Card>()).UseCard(Src, new List<Player> { dest1 });
            }
            // 摸牌
            else await new GetCardFromPile(dest, 1).Execute();
        }
    }
}
