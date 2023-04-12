using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class Equipage : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src);

            await AddEquipage(src);
        }

        public Player Owner { get; private set; }

        /// <summary>
        /// 置入装备区
        /// </summary>
        public virtual async Task AddEquipage(Player owner)
        {
            Owner = owner;
            addEquipView?.Invoke(this);

            if (Owner.Equipages[Type] != null)
            {
                CardPile.Instance.AddToDiscard(Owner.Equipages[Type]);
                await new LoseCard(Owner, new List<Card> { Owner.Equipages[Type] }).Execute();
            }
            Owner.Equipages[Type] = this;
        }

        /// <summary>
        /// 移出装备区(只由LoseCard调用)
        /// </summary>
        public virtual async Task RemoveEquipage()
        {
            await Task.Yield();
            removeEquipView?.Invoke(this);
            Owner.Equipages[Type] = null;
        }

        public void SkillView()
        {
            Debug.Log(Owner.posStr + "号位发动了" + Name + "【" + Suit + Weight.ToString() + "】");
        }


        private static UnityAction<Equipage> addEquipView;
        private static UnityAction<Equipage> removeEquipView;

        public static event UnityAction<Equipage> AddEquipView
        {
            add => addEquipView += value;
            remove => addEquipView -= value;
        }
        public static event UnityAction<Equipage> RemoveEquipView
        {
            add => removeEquipView += value;
            remove => removeEquipView -= value;
        }
    }

    public class PlusHorse : Equipage
    {
        public override async Task AddEquipage(Player owner)
        {
            owner.DstPlus++;
            await base.AddEquipage(owner);
        }
        public override async Task RemoveEquipage()
        {
            Owner.DstPlus--;
            await base.RemoveEquipage();
        }
    }

    public class SubHorse : Equipage
    {
        public override async Task AddEquipage(Player owner)
        {
            owner.DstSub++;
            await base.AddEquipage(owner);
        }
        public override async Task RemoveEquipage()
        {
            Owner.DstSub--;
            await base.RemoveEquipage();
        }
    }
}
