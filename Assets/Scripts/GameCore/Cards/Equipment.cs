using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameCore
{
    public class Equipment : Card, IExecutable
    {
        protected override async Task AfterInit()
        {
            await Add(src);
        }

        public Player owner { get; private set; }

        /// <summary>
        /// 置入装备区
        /// </summary>
        public virtual async Task Add(Player _owner)
        {
            owner = _owner;
            if (!MCTS.Instance.isRunning) AddEquipView?.Invoke(this);

            if (owner.Equipments.ContainsKey(type))
            {
                CardPile.Instance.AddToDiscard(owner.Equipments[type]);
                await new LoseCard(owner, new List<Card> { owner.Equipments[type] }).Execute();
            }
            owner.Equipments[type] = this;
        }

        /// <summary>
        /// 移出装备区(只由LoseCard调用)
        /// </summary>
        public virtual async Task Remove()
        {
            await Task.Yield();
            if (!MCTS.Instance.isRunning) RemoveEquipView?.Invoke(this);
            OnRemove?.Invoke();
            owner.Equipments.Remove(type);
            owner = null;
        }

        public void Execute()
        {
            Util.Print(owner + "发动了" + this);
        }

        public Action OnRemove { get; set; }

        public static Action<Equipment> AddEquipView { get; set; }
        public static Action<Equipment> RemoveEquipView { get; set; }

        public virtual bool enabled => true;
    }

    public class PlusHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.fleeDistance++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            owner.fleeDistance--;
            await base.Remove();
        }
    }

    public class SubHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.pursueDistance++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            owner.pursueDistance--;
            await base.Remove();
        }
    }

    public class 绝影 : PlusHorse { }
    public class 大宛 : SubHorse { }
    public class 赤兔 : SubHorse { }
    public class 爪黄飞电 : PlusHorse { }
    public class 的卢 : PlusHorse { }
    public class 紫骍 : SubHorse { }
    public class 骅骝 : PlusHorse { }
}
