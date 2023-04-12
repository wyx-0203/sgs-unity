using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Armor : Equipage
    {
        // public bool enable { get; set; }

        // public override async Task AddEquipage(Player owner)
        // {
        //     enable = true;
        //     await base.AddEquipage(owner);
        // }

        // public virtual bool Disable(Card card)
        // {
        //     return false;
        // }

        public virtual void WhenDamaged(Damaged damaged) { }
    }

    public class 八卦阵 : Armor
    {
        public async Task<bool> Skill()
        {
            // if (!enable) return false;

            Timer.Instance.Hint = "是否发动八卦阵？";
            bool result = await Timer.Instance.Run(Owner);
            if (!result && !Owner.isAI) return false;

            SkillView();
            var card = await new Judge().Execute();
            return card.Suit == "红桃" || card.Suit == "方片";
        }
    }

    public class 藤甲 : Armor
    {
        public override async Task AddEquipage(Player owner)
        {
            await base.AddEquipage(owner);
            Owner.disableForMe += Disable;
        }

        public override async Task RemoveEquipage()
        {
            await base.RemoveEquipage();
            Owner.disableForMe -= Disable;
        }
        public bool Disable(Card card) => card is 杀
            && card is not 雷杀
            && card is not 火杀
            && !(card as 杀).IgnoreArmor
            || card is 南蛮入侵
            || card is 万箭齐发;

        public override void WhenDamaged(Damaged damaged)
        {
            if (damaged.damageType == DamageType.Fire) damaged.Value--;
        }
    }

    public class 仁王盾 : Armor
    {
        public override async Task AddEquipage(Player owner)
        {
            await base.AddEquipage(owner);
            Owner.disableForMe += Disable;
        }

        public override async Task RemoveEquipage()
        {
            await base.RemoveEquipage();
            Owner.disableForMe -= Disable;
        }

        public bool Disable(Card card) => card is 杀
            && !(card as 杀).IgnoreArmor
            && (card.Suit == "黑桃" || card.Suit == "草花" || card.Suit == "黑色");
    }

    public class 白银狮子 : Armor
    {
        public override async Task RemoveEquipage()
        {
            await base.RemoveEquipage();
            await new Recover(Owner).Execute();
        }
        public override void WhenDamaged(Damaged damaged)
        {
            if (damaged.Value == -1) return;
            damaged.Value = -1;
        }
    }
}
