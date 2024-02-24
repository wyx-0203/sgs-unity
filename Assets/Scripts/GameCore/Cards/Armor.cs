using System.Threading.Tasks;

namespace GameCore
{
    public class Armor : Equipment { }

    public class 八卦阵 : Armor
    {
        public async Task<bool> Invoke(Card srcCard)
        {
            if (srcCard is 杀 sha && sha.ignoreArmor) return false;
            // Timer.Instance.hint = "是否发动八卦阵？";
            // Timer.Instance.equipSkill = this;
            // Timer.Instance.DefaultAI = () => new Decision { action = true };
            if (!(await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
            }.Run()).action) return false;

            Execute();
            var card = await Judge.Execute(src);
            return card.suit == "红桃" || card.suit == "方片";
        }
    }

    public class 藤甲 : Armor
    {
        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            base.owner.effects.InvalidForDest.Add(InvalidForDest, this);
            base.owner.effects.OffsetDamageValue.Add(OffsetDamageValue, this);
        }

        private bool InvalidForDest(Card card) => card is 杀 sha
            && card is not 雷杀
            && card is not 火杀
            && !sha.ignoreArmor
            || card is 南蛮入侵
            || card is 万箭齐发;

        private int OffsetDamageValue(Damage damage) => damage.type == Model.Damage.Type.Fire
            && (damage.SrcCard is not 火杀 sha || !sha.ignoreArmor) ? 1 : 0;
    }

    public class 仁王盾 : Armor
    {
        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            base.owner.effects.InvalidForDest.Add(x => x is 杀 sha && !sha.ignoreArmor && sha.isBlack, this);
        }
    }

    public class 白银狮子 : Armor
    {
        public override async Task Remove()
        {
            await new Recover(owner).Execute();
            await base.Remove();
        }
    }
}
