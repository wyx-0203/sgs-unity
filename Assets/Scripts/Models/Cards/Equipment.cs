using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class Equipment : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src);

            await Add(src);
        }

        public Player Owner { get; private set; }

        /// <summary>
        /// 置入装备区
        /// </summary>
        public virtual async Task Add(Player owner)
        {
            Owner = owner;
            AddEquipView?.Invoke(this);

            if (Owner.Equipments.ContainsKey(type))
            {
                CardPile.Instance.AddToDiscard(Owner.Equipments[type]);
                await new LoseCard(Owner, new List<Card> { Owner.Equipments[type] }).Execute();
            }
            Owner.Equipments[type] = this;
        }

        /// <summary>
        /// 移出装备区(只由LoseCard调用)
        /// </summary>
        public virtual async Task Remove()
        {
            await Task.Yield();
            RemoveEquipView?.Invoke(this);
            Owner.Equipments.Remove(type);
        }

        public void SkillView()
        {
            Debug.Log(Owner + "号位发动了" + this);
        }

        public static UnityAction<Equipment> AddEquipView { get; set; }
        public static UnityAction<Equipment> RemoveEquipView { get; set; }
    }

    public class PlusHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.DstPlus++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            Owner.DstPlus--;
            await base.Remove();
        }
    }

    public class SubHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.DstSub++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            Owner.DstSub--;
            await base.Remove();
        }
    }
}
