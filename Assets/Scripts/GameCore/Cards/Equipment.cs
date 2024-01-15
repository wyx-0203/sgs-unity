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

            if (owner.Equipments.ContainsKey(type))
            {
                CardPile.Instance.AddToDiscard(owner.Equipments[type], owner);
                await new LoseCard(owner, new List<Card> { owner.Equipments[type] }).Execute();
            }
            owner.Equipments[type] = this;

            EventSystem.Instance.Send(new Model.AddEquipment
            {
                player = owner.position,
                card = id
            });
        }

        /// <summary>
        /// 移出装备区(只由LoseCard调用)
        /// </summary>
        public virtual async Task Remove()
        {
            await Task.Yield();
            OnRemove?.Invoke();
            owner.Equipments.Remove(type);
            owner = null;
        }

        public void Execute()
        {
            Util.Print(owner + "发动了" + this);
        }

        protected string hint => $"是否发动{name}？";

        public Action OnRemove { get; set; }

        public virtual bool enabled => true;
    }

    public class PlusHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.plusDst++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            owner.plusDst--;
            await base.Remove();
        }
    }

    public class SubHorse : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.subDst++;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            owner.subDst--;
            await base.Remove();
        }
    }

    public class 绝影 : PlusHorse { public 绝影() { } }
    public class 大宛 : SubHorse { public 大宛() { } }
    public class 赤兔 : SubHorse { public 赤兔() { } }
    public class 爪黄飞电 : PlusHorse { public 爪黄飞电() { } }
    public class 的卢 : PlusHorse { public 的卢() { } }
    public class 紫骍 : SubHorse { public 紫骍() { } }
    public class 骅骝 : PlusHorse { public 骅骝() { } }
}
