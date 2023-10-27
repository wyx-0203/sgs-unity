using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
{
    public class Equipment : Card, Executable
    {
        protected override async Task AfterInit()
        {
            await Add(Src);
        }

        public Player Owner { get; private set; }

        /// <summary>
        /// 置入装备区
        /// </summary>
        public virtual async Task Add(Player owner)
        {
            Owner = owner;
            if (!MCTS.Instance.isRunning) AddEquipView?.Invoke(this);

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
            if (!MCTS.Instance.isRunning) RemoveEquipView?.Invoke(this);
            OnRemove?.Invoke();
            Owner.Equipments.Remove(type);
            Owner = null;
        }

        public void Execute()
        {
            Util.Print(Owner + "号位发动了" + this);
        }

        public Action OnRemove { get; set; }

        public static UnityAction<Equipment> AddEquipView { get; set; }
        public static UnityAction<Equipment> RemoveEquipView { get; set; }

        public virtual bool enabled => true;
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
