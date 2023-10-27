using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    /// <summary>
    /// 技能基类
    /// </summary>
    public class Skill : Executable
    {
        // 所属玩家
        public Player src { get; private set; }
        // 技能名称
        public string name { get; private set; }
        // 锁定技
        public virtual bool isObey => false;
        // 限定次数
        public virtual int timeLimit => int.MaxValue;
        // 已使用次数
        public int time { get; protected set; }

        protected virtual void Init(string name, Player src)
        {
            this.name = name;
            this.src = src;
            src.skills.Add(this);
            // EventSystem.Instance.skills.Add(this);
            TurnSystem.Instance.AfterTurn += ResetAfterTurn;
            TurnSystem.Instance.AfterPlay += ResetAfterPlay;
            // OnStart();
        }

        public virtual void Remove()
        {
            TurnSystem.Instance.AfterTurn -= ResetAfterTurn;
            TurnSystem.Instance.AfterPlay -= ResetAfterPlay;
            src.skills.Remove(this);
            // EventSystem.Instance.skills.Remove(this);
            // if (this is Triggered triggered) EventSystem.Instance.skills.Remove(triggered);
            OnRemove?.Invoke();
        }

        /// <summary>
        /// 最大可选卡牌数
        /// </summary>
        public virtual int MaxCard => 0;

        /// <summary>
        /// 最小可选卡牌数
        /// </summary>
        public virtual int MinCard => 0;

        /// <summary>
        /// 判断卡牌是否可选
        /// </summary>
        public virtual bool IsValidCard(Card card) => card.discardable;

        /// <summary>
        /// 最大目标数
        /// </summary>
        public virtual int MaxDest => 0;

        /// <summary>
        /// 最小目标数
        /// </summary>
        public virtual int MinDest => 0;

        /// <summary>
        /// 判断目标是否可选
        /// </summary>
        /// <param name="dest">目标</param>
        public virtual bool IsValidDest(Player dest) => true;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool enabled => !src.effects.DisableSkill.Invoke(this);
        // public bool enabled => !DisableSkill.Instance.Invoke(this);

        // protected virtual void OnStart() { }
        // protected virtual void OnDestroy() { }
        public Action OnRemove { get; set; }

        /// <summary>
        /// 技能是否满足条件
        /// </summary>
        public virtual bool IsValid => enabled && time < timeLimit && (this is not Ultimate ultimate || !ultimate.IsDone);

        public void Execute(Decision decision)
        {
            time++;
            // Dests = decision?.dests;
            // this.decision = decision;
            if (!MCTS.Instance.isRunning) UseSkillView?.Invoke(this, decision?.dests);
            string destStr = decision != null && decision.dests.Count > 0 ? "对" + string.Join("、", decision.dests) : "";
            Util.Print(src + destStr + "使用了" + name);
        }
        public void Execute() => Execute(null);

        public virtual async Task Invoke(object arg) { await Task.Yield(); }

        protected virtual void ResetAfterTurn() => time = 0;

        protected virtual void ResetAfterPlay() { }

        // public Decision decision { get; protected set; }

        protected Player firstDest => Timer.Instance.temp.dests.Count == 0 ? null : Timer.Instance.temp.dests[0];

        public static UnityAction<Skill, List<Player>> UseSkillView { get; set; }

        public virtual Decision AIDecision() => new Decision();

        public static Skill New(string name, Player src)
        {
            Debug.Log($"{Application.dataPath}/../HybridCLRData/HotUpdateDlls/Android/GeneralSkill.dll");
            Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "GeneralSkill");
            // if (hotUpdateAss is null)
            // hotUpdateAss = Assembly.Load(File.ReadAllBytes($"{Application.dataPath}/../HybridCLRData/HotUpdateDlls/Android/GeneralSkill.dll"));
            var skill = Activator.CreateInstance(hotUpdateAss.GetType(name)) as Skill;
            skill.Init(name, src);
            return skill;
        }

        // 阶段开始时调用
        public bool OnEveryPhaseStart(Tuple<Player, Phase> tuple) => OnEveryPhaseStart(tuple.Item1, tuple.Item2);
        protected virtual bool OnEveryPhaseStart(Player player, Phase phase) => player == src && OnPhaseStart(phase);
        protected virtual bool OnPhaseStart(Phase phase) => false;
        // 阶段结束时调用
        public bool OnEveryPhaseOver(Tuple<Player, Phase> tuple)
        {
            if (this is Active && tuple.Item2 == Phase.Play) time = 0;
            if (this is not Active && tuple.Item2 == Phase.End) time = 0;
            return OnEveryPhaseOver(tuple.Item1, tuple.Item2);
        }
        protected virtual bool OnEveryPhaseOver(Player player, Phase phase) => player == src && OnPhaseOver(phase);
        protected virtual bool OnPhaseOver(Phase phase) => false;
        // 摸牌时调用
        public virtual bool BeforeEveryGetCardFromPile(GetCardFromPile g) => g.player == src && BeforeGetCardFromPile(g);
        protected virtual bool BeforeGetCardFromPile(GetCardFromPile getCardFromPile) => getCardFromPile.inGetPhase && BeforeGetCardInGetPhase(getCardFromPile);
        protected virtual bool BeforeGetCardInGetPhase(GetCardFromPile getCardFromPile) => false;
        // 获得牌后调用
        public bool OnEveryGetCard(GetCard getCard) => TurnSystem.Instance.Round > 0
            && (getCard.player == src && OnGetCard(getCard)
            || getCard is GetCardFromElse getCardFromElse && OnEveryGetCardFromElse(getCardFromElse));
        protected virtual bool OnEveryGetCardFromElse(GetCardFromElse getCardFromElse) => getCardFromElse.player == src && OnGetCardFromElse(getCardFromElse);
        protected virtual bool OnGetCard(GetCard getCard) => false;
        protected virtual bool OnGetCardFromElse(GetCardFromElse getCardFromElse) => false;
        // 失去牌后调用
        public virtual bool OnEveryLoseCard(LoseCard loseCard) => loseCard.player == src && OnLoseCard(loseCard);
        protected virtual bool OnLoseCard(LoseCard loseCard) => false;
        // 回复体力后调用
        public bool OnEveryUpdateHp(UpdateHp updateHp)
            // 回复体力
            => updateHp is Recover recover && OnEveryRecover(recover)
            // 受到伤害
            || updateHp is Damaged damaged && OnEveryDamaged(damaged)
            // 失去体力
            || updateHp.value < 0 && OnEveryLoseHp(updateHp)
            // 改变体力
            || updateHp.player == src && OnUpdateHp(updateHp);
        protected virtual bool OnUpdateHp(UpdateHp updateHp) => false;

        public virtual bool OnEveryRecover(Recover recover) => recover.player == src && OnRecover(recover);
        protected virtual bool OnRecover(Recover recover) => false;
        // 受到伤害时调用
        public virtual bool BeforeEveryDamaged(Damaged damaged) => damaged.player == src && BeforeDamaged(damaged);
        protected virtual bool BeforeDamaged(Damaged damaged) => false;
        // 受到伤害后调用
        public virtual bool OnEveryDamaged(Damaged damaged) => damaged.player == src && OnDamaged(damaged);
        protected virtual bool OnDamaged(Damaged damaged) => false;
        // 失去体力后调用
        public virtual bool OnEveryLoseHp(UpdateHp updateHp) => updateHp.player == src && OnLoseHp(updateHp);
        protected virtual bool OnLoseHp(UpdateHp updateHp) => false;
        // 使用牌时调用
        public virtual bool OnEveryUseCard(Card card) => card.Src == src && OnUseCard(card);
        protected virtual bool OnUseCard(Card card) => false;
        // 使用牌后调用
        public virtual bool AfterUseCard(Card card) => card.Src == src && AfterSetCardTarget(card);
        protected virtual bool AfterSetCardTarget(Card card) => false;
        // 翻面时调用
        public virtual bool OnEveryTurnOver(TurnOver turnOver) => turnOver.player == src && OnTurnOver(turnOver);
        protected virtual bool OnTurnOver(TurnOver turnOver) => false;

        public virtual bool OnShaUsing(杀 sha) => false;
        public virtual bool OnEveryJuedouUsing(决斗 juedou) => false;
    }

    /// <summary>
    /// 限定技
    /// </summary>
    public interface Ultimate
    {
        public bool IsDone { get; set; }
    }


    // public interface
}