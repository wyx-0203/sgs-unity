using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameCore
{
    public class Weapon : Equipment
    {
        public override async Task Add(Player owner)
        {
            owner.attackRange += range - 1;
            await base.Add(owner);
        }
        public override async Task Remove()
        {
            owner.attackRange -= range - 1;
            await base.Remove();
        }

        /// <summary>
        /// 攻击范围
        /// </summary>
        protected int range;

        public virtual async Task BeforeUseSha(杀 sha) => await Task.Yield();

        public virtual async Task AfterInitSha(杀 sha) => await Task.Yield();

        public virtual async Task OnShaMissed(杀 sha) => await Task.Yield();

        public virtual async Task OnShaDamage(杀 sha) => await Task.Yield();
    }

    public class 青龙偃月刀 : Weapon
    {
        public 青龙偃月刀()
        {
            range = 3;
        }

        public override async Task OnShaMissed(杀 sha)
        {
            Timer.Instance.equipSkill = this;
            Timer.Instance.hint = "是否发动青龙偃月刀？";
            Timer.Instance.isValidCard = card => card is 杀;
            Timer.Instance.isValidDest = player => player == sha.dest;
            Timer.Instance.DefaultAI = owner.team != sha.dest.team || !AI.CertainValue ? AI.TryAction : () => new Decision();
            var decision = await Timer.Instance.Run(owner, 1, 1);

            if (!decision.action) return;

            Execute();
            await decision.cards[0].UseCard(owner, new List<Player> { sha.dest });
        }
    }

    public class 麒麟弓 : Weapon
    {
        public 麒麟弓()
        {
            range = 5;
        }

        public override async Task OnShaDamage(杀 sha)
        {
            var dest = sha.dest;
            if (dest.plusHorse is null && dest.subHorse is null) return;

            Timer.Instance.equipSkill = this;
            Timer.Instance.hint = "是否发动麒麟弓？";
            Timer.Instance.DefaultAI = () => new Decision { action = owner.team != dest.team };

            var decision = await Timer.Instance.Run(owner);
            if (!decision.action) return;
            Execute();

            var list = new List<Card>();
            if (dest.plusHorse != null) list.Add(dest.plusHorse);
            if (dest.subHorse != null) list.Add(dest.subHorse);

            CardPanel.Instance.Title = "麒麟弓";
            decision = await CardPanel.Instance.Run(owner, dest, list);

            await new Discard(dest, decision.cards).Execute();
        }
    }

    public class 雌雄双股剑 : Weapon
    {
        public 雌雄双股剑()
        {
            range = 2;
        }

        public override async Task AfterInitSha(杀 sha)
        {
            foreach (var i in sha.dests)
            {
                if (i.general.gender == sha.src.general.gender) continue;
                Timer.Instance.equipSkill = this;
                Timer.Instance.hint = "是否对" + i + "发动雌雄双股剑？";
                Timer.Instance.DefaultAI = AI.TryAction;

                if (!(await Timer.Instance.Run(owner)).action) continue;
                Execute();

                if (i.handCardsCount == 0)
                {
                    await new GetCardFromPile(sha.src, 1).Execute();
                    continue;
                }

                Timer.Instance.hint = src + "对你发动雌雄双股剑，请弃一张手牌或令其摸一张牌";
                Timer.Instance.isValidCard = card => card.isHandCard;
                var decision = await Timer.Instance.Run(i, 1, 0);
                if (decision.action) await new Discard(i, decision.cards).Execute();
                else await new GetCardFromPile(sha.src, 1).Execute();
            }
        }
    }

    public class 青釭剑 : Weapon
    {
        public 青釭剑()
        {
            range = 2;
        }

        public override async Task BeforeUseSha(杀 sha)
        {
            await Task.Yield();
            Execute();
            sha.ignoreArmor = true;
        }
    }

    public class 丈八蛇矛 : Weapon
    {
        public 丈八蛇矛()
        {
            range = 3;
        }

        public override async Task Add(Player owner)
        {
            skill = new _Skill(owner);
            await base.Add(owner);
        }

        public override async Task Remove()
        {
            skill.Remove();
            skill = null;
            await base.Remove();
        }

        public _Skill skill { get; private set; }

        public class _Skill : Converted
        {
            public override Card Convert(List<Card> cards) => Card.Convert<杀>(src, cards);
            public override bool IsValidCard(Card card) => card.isHandCard;
            public override int MaxCard => 2;
            public override int MinCard => 2;
            public _Skill(Player src) => Init("丈八蛇矛", src);
        }
    }

    public class 诸葛连弩 : Weapon
    {

        public 诸葛连弩()
        {
            range = 1;
        }

        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            base.owner.effects.NoTimesLimit.Add(x => x is 杀, this);
        }
    }

    public class 贯石斧 : Weapon
    {
        public 贯石斧()
        {
            range = 3;
        }

        public override async Task OnShaMissed(杀 sha)
        {
            Timer.Instance.equipSkill = this;
            Timer.Instance.hint = "是否发动贯石斧？";
            Timer.Instance.isValidCard = card => card != owner.weapon && card.discardable;
            Timer.Instance.DefaultAI = sha.dest.team != owner.team ? AI.TryAction : () => new();

            var decision = await Timer.Instance.Run(owner, 2, 0);
            if (!decision.action) return;

            await new Discard(owner, decision.cards).Execute();
            sha.isDamage = true;
        }
    }

    public class 方天画戟 : Weapon
    {
        public 方天画戟()
        {
            range = 4;
        }

        public override async Task Add(Player owner)
        {
            await base.Add(owner);
            src.effects.ExtraDestCount.Add(x => x.isHandCard && base.owner.handCardsCount == 1 ? 2 : 0, this);
        }
    }

    public class 朱雀羽扇 : Weapon
    {
        public 朱雀羽扇()
        {
            range = 4;
        }

        public override async Task BeforeUseSha(杀 sha)
        {
            if (sha is 火杀 || sha is 雷杀) return;

            Timer.Instance.equipSkill = this;
            Timer.Instance.hint = "是否发动朱雀羽扇？";
            Timer.Instance.DefaultAI = () => new Decision { action = true };
            if (!(await Timer.Instance.Run(owner)).action) return;

            Execute();
            await Card.Convert<火杀>(sha.src, new List<Card> { sha }).UseCard(sha.src, sha.dests);
            throw new CancelUseCard();
        }
    }

    public class 古锭刀 : Weapon
    {
        public 古锭刀()
        {
            range = 2;
        }

        public override async Task OnShaDamage(杀 sha)
        {
            if (sha.dest.handCardsCount > 0) return;
            await Task.Yield();
            Execute();
            sha.AddDamageValue(sha.dest, 1);
        }
    }

    public class 寒冰剑 : Weapon
    {
        public 寒冰剑()
        {
            range = 2;
        }

        public override async Task OnShaDamage(杀 sha)
        {
            var dest = sha.dest;
            if (dest.cardsCount == 0) return;
            Timer.Instance.equipSkill = this;
            Timer.Instance.hint = "是否发动寒冰剑？";
            Timer.Instance.DefaultAI = () => new Decision { action = !(dest.team != src.team && dest.cardsCount < 2) && UnityEngine.Random.value < 0.5f };
            if (!(await Timer.Instance.Run(owner)).action) return;

            Execute();
            var card = await TimerAction.SelectOneCardFromElse(owner, dest);
            await new Discard(dest, card).Execute();

            if (dest.cardsCount > 0)
            {
                card = await TimerAction.SelectOneCardFromElse(owner, dest);
                await new Discard(dest, card).Execute();
            }

            throw new PreventDamage();
        }
    }
}
