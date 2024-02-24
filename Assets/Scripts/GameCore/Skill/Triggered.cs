using System;
using System.Linq;
using System.Threading.Tasks;
using Phase = Model.Phase;

namespace GameCore
{
    public abstract class Triggered : Skill
    {
        /// <summary>
        /// 询问是否发动技能
        /// </summary>
        public async Task<PlayDecision> WaitDecision()
        {
            // Timer.Instance.temp.skill = this;
            // Timer.Instance.hint = "是否发动" + name + "？";
            // Timer.Instance.defaultAI = AIDecision;
            return await ToPlayQuery(null).Run();
        }

        // public override Decision AIDecision() => AI.TryAction();
        public override bool AIAct => true;
        protected abstract Task Invoke(PlayDecision decision);
        protected object arg;

        public static async Task Invoke<T>(Game game, Func<Triggered, Predicate<T>> func, T arg, Action afterInvoke = null)
        {
            foreach (var i in game.AlivePlayers.OrderBy(x => x.orderKey))
            {
                // var triggereds = i.skills.Where(x => x is Triggered t && t.IsValid && func(t)(arg)).Cast<Triggered>().ToList();
                // // if(triggereds.Count==0) continue;
                // // else if(triggereds.Count==1){
                // //     // 
                // // }
                // while(triggereds.Count>0){
                //     // 
                // }
                foreach (var triggered in i.skills.Where(x => x is Triggered t && t.IsValid && func(t)(arg)).Cast<Triggered>().ToList())
                {
                    triggered.arg = arg;
                    PlayDecision decision = null;
                    if (!triggered.passive)
                    {
                        decision = await triggered.WaitDecision();
                        if (!decision.action) continue;
                    }
                    triggered.Execute(decision);
                    await triggered.Invoke(decision);
                    afterInvoke?.Invoke();
                }
            }
        }


        public bool OnEveryPhaseStart(Tuple<Player, Phase> tuple) => OnEveryPhaseStart(tuple.Item1, tuple.Item2);

        /// <summary>
        /// 所有人阶段开始时调用
        /// </summary>
        protected virtual bool OnEveryPhaseStart(Player player, Phase phase) => player == src && OnPhaseStart(phase);

        /// <summary>
        /// 阶段开始时调用
        /// </summary>
        protected virtual bool OnPhaseStart(Phase phase) => false;

        public bool OnEveryPhaseOver(Tuple<Player, Phase> tuple) => OnEveryPhaseOver(tuple.Item1, tuple.Item2);

        /// <summary>
        /// 所有人阶段结束时调用
        /// </summary>
        protected virtual bool OnEveryPhaseOver(Player player, Phase phase) => player == src && OnPhaseOver(phase);

        /// <summary>
        /// 阶段结束时调用
        /// </summary>
        protected virtual bool OnPhaseOver(Phase phase) => false;

        /// <summary>
        /// 所有人摸牌前调用
        /// </summary>
        public virtual bool BeforeEveryDrawCard(DrawCard g) => g.player == src && BeforeDrawCard(g);

        /// <summary>
        /// 摸牌前调用
        /// </summary>
        protected virtual bool BeforeDrawCard(DrawCard drawCard) => drawCard.inGetPhase && BeforeDrawInDrawPhase(drawCard);

        /// <summary>
        /// 摸牌阶段摸牌前调用
        /// </summary>
        protected virtual bool BeforeDrawInDrawPhase(DrawCard drawCard) => false;

        public bool OnEveryGetCard(GetCard getCard) =>
            // game.turnSystem.round > 0&& 
            getCard.player == src && OnGetCard(getCard);
        // || getCard is GetAnothersCard getCardFromElse && OnEveryGetAnothersCard(getCardFromElse));

        public virtual bool OnEveryDrawCard(DrawCard drawCard) => OnEveryGetCard(drawCard);

        public virtual bool OnEveryGetAnothersCard(GetAnothersCard getAnothersCard) => OnEveryGetCard(getAnothersCard)
            || getAnothersCard.player == src
            && OnGetAnothersCard(getAnothersCard);

        /// <summary>
        /// 获得牌后调用
        /// </summary>
        protected virtual bool OnGetCard(GetCard getCard) => false;

        /// <summary>
        /// 获得其他角色的牌后调用
        /// </summary>
        protected virtual bool OnGetAnothersCard(GetAnothersCard getAnothersCard) => false;

        /// <summary>
        /// 所有人失去牌后调用
        /// </summary>
        public virtual bool OnEveryLoseCard(LoseCard loseCard) => loseCard.player == src && OnLoseCard(loseCard);
        public virtual bool OnEveryDiscard(Discard discard) => OnEveryLoseCard(discard);

        /// <summary>
        /// 失去牌后调用
        /// </summary>
        protected virtual bool OnLoseCard(LoseCard loseCard) => false;

        public bool OnEveryUpdateHp(UpdateHp updateHp)
            // 回复体力
            =>
             //  updateHp is Recover recover && OnEveryRecover(recover)
             // // 受到伤害
             // || updateHp is Damaged damaged && OnEveryDamaged(damaged)
             // // 失去体力
             // || updateHp is not Damaged && updateHp.value < 0 && OnEveryLoseHp(updateHp)
             // // 改变体力
             // ||
             updateHp.player == src && OnUpdateHp(updateHp);

        /// <summary>
        /// 体力值改变后调用
        /// </summary>
        protected virtual bool OnUpdateHp(UpdateHp updateHp) => false;

        /// <summary>
        /// 所有人回复体力后调用
        /// </summary>
        public virtual bool OnEveryRecover(Recover recover) => OnEveryUpdateHp(recover) || recover.player == src && OnRecover(recover);

        /// <summary>
        /// 回复体力后调用
        /// </summary>
        protected virtual bool OnRecover(Recover recover) => false;

        /// <summary>
        /// 所有人受到伤害时调用
        /// </summary>
        public virtual bool BeforeEveryDamaged(Damage damaged) =>
            damaged.player == src
            && BeforeDamaged(damaged)
            || damaged.player != src
            && damaged.Src == src
            && BeforeMakeDamage(damaged);

        /// <summary>
        /// 受到伤害时调用
        /// </summary>
        protected virtual bool BeforeDamaged(Damage damaged) => false;

        /// <summary>
        /// 对其他角色造成伤害时调用
        /// </summary>
        protected virtual bool BeforeMakeDamage(Damage damaged) => false;

        /// <summary>
        /// 所有人受到伤害后调用
        /// </summary>
        public virtual bool OnEveryDamaged(Damage damaged) =>
            damaged.player == src
            && OnDamaged(damaged)
            || damaged.player != src
            && damaged.Src == src
            && OnMakeDamage(damaged)
            || OnEveryUpdateHp(damaged);

        /// <summary>
        /// 对其他角色造成伤害后调用
        /// </summary>
        protected virtual bool OnMakeDamage(Damage damaged) => false;

        /// <summary>
        /// 受到伤害后调用
        /// </summary>
        protected virtual bool OnDamaged(Damage damaged) => damaged.Src != src && damaged.Src != null && OnDamagedByElse(damaged);

        /// <summary>
        /// 受到其他角色造成的伤害后调用
        /// </summary>
        protected virtual bool OnDamagedByElse(Damage damaged) => false;

        public bool OnEveryLoseHp(LoseHp loseHp) => loseHp.player == src && OnLoseHp(loseHp) || OnEveryUpdateHp(loseHp);

        /// <summary>
        /// 失去体力后调用
        /// </summary>
        protected virtual bool OnLoseHp(LoseHp loseHp) => false;

        /// <summary>
        /// 所有人使用牌时调用
        /// </summary>
        public virtual bool OnEveryUseCard(Card card) => card.src == src && OnUseCard(card);

        /// <summary>
        /// 使用牌时调用
        /// </summary>
        protected virtual bool OnUseCard(Card card) => false;

        /// <summary>
        /// 所有人使用牌指定目标后调用
        /// </summary>
        public virtual bool AfterEveryUseCard(Card card) => card.src == src && AfterUseCard(card);

        /// <summary>
        /// 使用牌指定目标后调用
        /// </summary>
        protected virtual bool AfterUseCard(Card card) => false;

        public virtual bool OnEveryExecuteSha(杀 sha) => sha.src == src && OnExecuteSha(sha);
        protected virtual bool OnExecuteSha(杀 sha) => false;

        /// <summary>
        /// 所有人翻面后调用
        /// </summary>
        public virtual bool OnEveryTurnOver(TurnOver turnOver) => turnOver.player == src && OnTurnOver(turnOver);

        /// <summary>
        /// 翻面后调用
        /// </summary>
        protected virtual bool OnTurnOver(TurnOver turnOver) => false;

        public bool OnEveryNearDeath(Player player) => player == src && OnNearDeath();

        /// <summary>
        /// 濒死状态时调用
        /// </summary>
        protected virtual bool OnNearDeath() => false;

        public bool OnEveryDie(Player player) => player == src && OnDie();

        /// <summary>
        /// 死亡时调用
        /// </summary>
        protected virtual bool OnDie() => false;
    }

    public abstract class Awoken : Triggered
    {
        public override bool passive => true;
    }
}
