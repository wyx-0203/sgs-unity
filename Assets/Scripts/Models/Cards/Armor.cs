using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Armor : Equipment
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

        // public virtual void WhenDamaged(Damaged damaged) { }
    }

    public class 八卦阵 : Armor
    {
        public async Task<bool> Skill()
        {
            Timer.Instance.hint = "是否发动八卦阵？";
            Timer.Instance.equipSkill = this;
            // Timer.Instance.equipSkill="八卦阵";
            Timer.Instance.DefaultAI = () => new Decision { action = true };
            if (!(await Timer.Instance.Run(Owner)).action) return false;

            Execute();
            var card = await new Judge().Execute();
            return card.suit == "红桃" || card.suit == "方片";
        }
    }

    public class 藤甲 : Armor
    {
        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            Owner.effects.InvalidForDest.Add(InvalidForDest, this);
            Owner.effects.OffsetDamageValue.Add(OffsetDamageValue, this);
        }

        private bool InvalidForDest(Card card) => card is 杀 sha
            && card is not 雷杀
            && card is not 火杀
            && !sha.IgnoreArmor
            || card is 南蛮入侵
            || card is 万箭齐发;

        private int OffsetDamageValue(Damaged damage) => damage.type == Damaged.Type.Fire
            && (damage.SrcCard is not 火杀 sha || !sha.IgnoreArmor) ? 1 : 0;
    }

    public class 仁王盾 : Armor
    {
        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            Owner.effects.InvalidForDest.Add(x => x is 杀 sha && !sha.IgnoreArmor && sha.isBlack, this);
        }
    }

    public class 白银狮子 : Armor
    {
        public override async Task Remove()
        {
            await new Recover(Owner).Execute();
            await base.Remove();
        }
    }
}
