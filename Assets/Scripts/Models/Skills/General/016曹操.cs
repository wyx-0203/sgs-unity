using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 奸雄 : Triggered
    {
        public 奸雄(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.events.AfterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.events.AfterDamaged.RemoveEvent(Src);
        }

        public async Task Execute(Damaged damaged)
        {
            if (!await base.ShowTimer()) return;
            Execute();

            // var srcCard = damaged.SrcCard;
            // List<Card> srcCard = null;
            if (damaged.SrcCard != null) await new GetDisCard(Src, new List<Card> { damaged.SrcCard }).Execute();

            // if (srcCard != null && srcCard.Count != 0) await new GetCard(Src, srcCard).Execute();
            await new GetCardFromPile(Src, 1).Execute();
        }
    }
}
