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

        public virtual Task BeforeUseSha(杀 sha) => Task.CompletedTask;

        public virtual Task AfterInitSha(杀 sha) => Task.CompletedTask;

        public virtual Task OnShaMissed(杀 sha) => Task.CompletedTask;

        public virtual Task OnShaDamage(杀 sha) => Task.CompletedTask;
    }

    public class 青龙偃月刀 : Weapon
    {
        public 青龙偃月刀()
        {
            range = 3;
        }

        public override async Task OnShaMissed(杀 sha)
        {
            // Timer.Instance.equipSkill = this;
            // Timer.Instance.hint = "是否发动青龙偃月刀？";
            // Timer.Instance.isValidCard = card => card is 杀;
            // Timer.Instance.isValidDest = player => player == sha.dest;
            // Timer.Instance.defaultAI = owner.team != sha.dest.team || !AI.CertainValue ? AI.TryAction : () => new Decision();
            // var decision = await Timer.Instance.Run(owner, 1, 1);
            var decision = await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
                isValidCard = card => card is 杀,
                isValidDest = player => player == sha.dest,
                aiAct = owner.team != sha.dest.team
            }.Run(1, 1);

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

            // Timer.Instance.equipSkill = this;
            // Timer.Instance.hint = "是否发动麒麟弓？";
            // Timer.Instance.defaultAI = () => new Decision { action = owner.team != dest.team };

            // var decision = await Timer.Instance.Run(owner);
            var decision = await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
                aiAct = owner.team != dest.team
            }.Run();
            if (!decision.action) return;
            Execute();

            var list = new List<Card>();
            if (dest.plusHorse != null) list.Add(dest.plusHorse);
            if (dest.subHorse != null) list.Add(dest.subHorse);
            // list=dest.Equipments.Values.Where(x=>x is PlusHorse||x is SubHorse).ToList();

            // CardPanelRequest.Instance.title = "麒麟弓";
            // decision = await CardPanelRequest.Instance.Run(owner, dest, list);
            var cards = await new CardPanelQuery(owner, dest, name, "请选择一张坐骑牌。", list).Run();

            await new Discard(dest, cards).Execute();
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
                // Timer.Instance.equipSkill = this;
                // Timer.Instance.hint = "是否对" + i + "发动雌雄双股剑？";
                // Timer.Instance.defaultAI = AI.TryAction;

                if (!(await new PlayQuery
                {
                    hint = $"是否对{i}发动雌雄双股剑？",
                    skill = name,
                }.Run()).action) continue;
                Execute();

                if (i.handCardsCount == 0)
                {
                    await new DrawCard(sha.src, 1).Execute();
                    continue;
                }

                // Timer.Instance.hint = src + "对你发动雌雄双股剑，请弃一张手牌或令其摸一张牌";
                // Timer.Instance.isValidCard = card => card.isHandCard;
                var decision = await new PlayQuery
                {
                    player = i,
                    hint = $"{src}对你发动雌雄双股剑，请弃一张手牌或令其摸一张牌",
                    isValidCard = card => card.isHandCard
                }.Run(1, 0);
                if (decision.action) await new Discard(i, decision.cards).Execute();
                else await new DrawCard(sha.src, 1).Execute();
            }
        }
    }

    public class 青釭剑 : Weapon
    {
        public 青釭剑()
        {
            range = 2;
        }

        public override Task BeforeUseSha(杀 sha)
        {
            Execute();
            sha.ignoreArmor = true;
            return Task.CompletedTask;
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
            var decision = await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
                isValidCard = card => card != this && card.discardable,
                aiAct = sha.dest.team != owner.team
            }.Run(2, 0);
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

            if (!(await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
            }.Run()).action) return;

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

        public override Task OnShaDamage(杀 sha)
        {
            if (sha.dest.handCardsCount == 0)
            {
                Execute();
                sha.AddDamageValue(sha.dest, 1);
            }
            return Task.CompletedTask;
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
            if (!(await new PlayQuery
            {
                player = owner,
                hint = hint,
                skill = name,
                aiAct = dest.cardsCount == 1 ^ dest.team != owner.team,
            }.Run()).action) return;

            Execute();
            string hint1 = $"对{dest}发动寒冰剑，依次弃置其两张牌";
            var cards = await new CardPanelQuery(owner, dest, name, hint1, false).Run();
            await new Discard(dest, cards).Execute();

            if (dest.cardsCount > 0)
            {
                cards = await new CardPanelQuery(owner, dest, name, hint1, false).Run();
                await new Discard(dest, cards).Execute();
            }

            throw new PreventDamage();
        }
    }
}
