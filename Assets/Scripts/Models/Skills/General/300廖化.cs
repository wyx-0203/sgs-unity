using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{

    public class 当先 : Triggered
    {
        public 当先(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            Src.events.StartPhase[Phase.Prepare].AddEvent(Src, Execute);
            Src.events.StartPhase[Phase.Perform].AddEvent(Src, StartPerform);
        }

        public override void OnDisable()
        {
            Src.events.StartPhase[Phase.Prepare].RemoveEvent(Src);
            Src.events.StartPhase[Phase.Perform].RemoveEvent(Src);
        }

        public new async Task Execute()
        {
            base.Execute();
            TurnSystem.Instance.ExtraPhase.Add(Phase.Perform);
            inSkill = true;
            await Task.Yield();
        }

        private bool inSkill;
        private bool change;

        public async Task StartPerform()
        {
            if (!inSkill) return;
            inSkill = false;
            if (change)
            {
                Timer.Instance.Hint = "是否失去1点体力并从弃牌堆获得一张【杀】？";
                if (!await Timer.Instance.Run(Src)) return;
            }

            await new UpdateHp(Src, -1).Execute();
            var card = CardPile.Instance.discardPile.Find(x => x is 杀);
            if (card != null) await new GetDisCard(Src, new List<Card> { card }).Execute();
        }
    }

    // public class 伏枥 : Triggered
    // {
    //     public 伏枥(Player src) : base(src){}
    //     public override bool Ultimate => true;

    //     // 
    // }
}