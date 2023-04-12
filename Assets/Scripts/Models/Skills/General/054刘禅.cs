using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 享乐 : Triggered
    {
        public 享乐(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.events.AfterUseCard.AddEvent(Src, Execute);
            }
            Src.disableForMe += DisableForMe;
        }

        public override void OnDisable()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.events.AfterUseCard.RemoveEvent(Src);
            }
            Src.disableForMe -= DisableForMe;
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀 || !card.Dests.Contains(Src)) return;
            Execute();

            Timer.Instance.Hint = "请弃置一张基本牌，否则此【杀】对刘禅无效。";
            Timer.Instance.IsValidCard = x => x.Type == "基本牌" && !x.IsConvert;
            bool result = await Timer.Instance.Run(card.Src, 1, 0);

            if (card.Src.isAI && Src.isSelf)
            {
                var c = card.Src.HandCards.Find(x => x.Type == "基本牌" && !x.IsConvert);
                if (c != null)
                {
                    Timer.Instance.Cards.Add(c);
                    result = true;
                }
            }

            if (result) await new Discard(card.Src, Timer.Instance.Cards).Execute();
            disableForMe = !result;
        }

        private bool disableForMe;

        public bool DisableForMe(Card card) => card is 杀 && disableForMe;
    }

    public class 放权 : Triggered
    {
        public 放权(Player src) : base(src) { }

        // public override int MaxDest => 1;
        // public override int MinDest => 1;
        // public override bool IsValidDest(Player dest1) => dest1 !=Src;

        // private Player dest;

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Perform].AddEvent(Src, Execute1);
            Src.events.StartPhase[Phase.End].AddEvent(Src, Execute2);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Perform].RemoveEvent(Src);
            Src.events.StartPhase[Phase.End].RemoveEvent(Src);
        }

        private bool use = false;

        public async Task Execute1()
        {

            // 触发条件
            if (!await base.ShowTimer()) return;
            Execute();
            TurnSystem.Instance.SkipPhase[Src][Phase.Perform] = true;
            use = true;
        }

        public async Task Execute2()
        {
            if (!use) return;
            Timer.Instance.Hint = "弃置一张手牌并令一名其他角色获得一个额外的回合";
            Timer.Instance.IsValidCard = x => Src.HandCards.Contains(x);
            Timer.Instance.IsValidDest = x => x != Src;
            if (!await Timer.Instance.Run(Src, 1, 1)) return;

            await new Discard(Src, Timer.Instance.Cards).Execute();
            TurnSystem.Instance.ExtraTurn = Timer.Instance.Dests[0];
        }

        protected override bool AIResult() => false;
    }
}
