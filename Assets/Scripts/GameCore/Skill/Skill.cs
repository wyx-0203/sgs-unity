using System;
using System.Collections.Generic;
using System.Linq;
using Team = Model.Team;

namespace GameCore
{
    /// <summary>
    /// 技能基类
    /// </summary>
    public class Skill : IExecutable
    {
        // 所属玩家
        public Player src { get; private set; }
        // 技能名称
        public string name { get; private set; }
        /// <summary>
        /// 锁定技
        /// </summary>
        public virtual bool passive => false;
        /// <summary>
        /// 每回合限定次数
        /// </summary>
        public virtual int timeLimit => int.MaxValue;
        // 已使用次数
        public int time { get; protected set; }
        protected Game game => src.game;

        protected void Init(string name, Player src)
        {
            this.name = name;
            this.src = src;
            src.skills.Add(this);
            if (this is Durative durative) durative.OnStart();
        }

        public void Remove()
        {
            src.skills.Remove(this);
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
        public virtual bool IsValidDest(Player dest) => true;

        public virtual bool IsValidSecondDest(Player dest, Player firstDest) => false;

        public virtual PlayQuery ToPlayQuery(PlayQuery origin) => new PlayQuery
        {
            player = src,
            skill = name,
            hint = $"是否发动【{name}】？",
            type = type,
            maxCard = MaxCard,
            minCard = MinCard,
            isValidCard = IsValidCard,
            maxDest = MaxDest,
            minDest = MinDest,
            isValidDest = IsValidDest,
            // 第二个目标(明策、眩惑)。若未被重写，则为空
            isValidSecondDest = GetType().GetMethod("IsValidSecondDest").DeclaringType == GetType() ? IsValidSecondDest : null,
            defaultAI = AIDecision,
            aiAct = AIAct
        };

        public virtual Model.SinglePlayQuery.Type type => Model.SinglePlayQuery.Type.Normal;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool enabled => !src.effects.DisableSkill.Invoke(this);

        public Action OnRemove { get; set; }

        /// <summary>
        /// 技能是否满足条件
        /// </summary>
        public virtual bool IsValid => enabled && time < timeLimit && (this is not Limited ultimate || !ultimate.IsDone);

        public void Execute(PlayDecision decision)
        {
            if (time == 0)
            {
                if (this is Active) game.turnSystem.AfterPlay += () => time = 0;
                else game.turnSystem.AfterTurn += () => time = 0;
            }
            time++;

            // if (!MCTS.Instance.isRunning) UseSkillView?.Invoke(this, decision?.dests);
            string destStr = decision != null && decision.dests.Count > 0 ? "对" + string.Join("、", decision.dests) : "";
            game.eventSystem.SendToClient(new Model.UseSkill
            {
                player = src.position,
                dests = decision?.dests.Select(x => x.position).ToList() ?? new(),
                skill = name,
                text = $"{src}{destStr}使用了{name}"
            });
            // Util.Print(src + destStr + "使用了" + name);
        }
        public void Execute() => Execute(null);

        // protected Player firstDest => Timer.Instance.temp.dests.Count == 0 ? null : Timer.Instance.temp.dests[0];

        // public static Action<Skill, List<Player>> UseSkillView { get; set; }

        /// <summary>
        /// AI如何发动技能，默认为随机指定手牌，随机指定任意名敌人
        /// </summary>
        /// <returns></returns>
        public virtual PlayDecision AIDecision() => AIUseToEnemy();
        /// <summary>
        /// AI是否发动技能，默认为true
        /// </summary>
        public virtual bool AIAct => true;

        protected IEnumerable<Player> AIGetAllDests() => game.AlivePlayers.Where(IsValidDest).Shuffle();
        protected IEnumerable<Player> AIGetDestsByTeam(Team team) => AIGetAllDests().Where(x => x.team == team);
        // .Take(MaxDest)
        // .ToList();
        protected List<Card> AIGetCards() => src.cards
            .Where(IsValidCard)
            .Shuffle(new Random().Next(MinCard, MaxCard + 1));

        protected PlayDecision AIUseToEnemy() => new PlayDecision
        {
            cards = AIGetCards(),
            dests = AIGetDestsByTeam(~src.team).Take(MaxDest).ToList()
        };

        protected PlayDecision AIUseToTeammate() => new PlayDecision
        {
            cards = AIGetCards(),
            dests = AIGetDestsByTeam(src.team).Take(MaxDest).ToList()
        };

        public static void New(string name, Player src)
        {
            // Debug.Log($"{Application.dataPath}/../HybridCLRData/HotUpdateDlls/Android/GeneralSkill.dll");
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Skills");
            // if (hotUpdateAss is null)
            // {
            //     hotUpdateAss = Assembly.Load(await WebRequest.GetBytes(Url.STATIC + "Dll/Skills.dll"));
            //     List<string> dlls = new List<string>
            //     {
            //         "GameCore.dll",
            //         // "System.Core.dll",
            //         // "UnityEngine.CoreModule.dll",
            //         // "Utils.dll",
            //         "mscorlib.dll",
            //     };
            //     foreach (var i in dlls)
            //     {
            //         var dllBytes = await WebRequest.GetBytes(Url.STATIC + "Dll/" + i);
            //         var err = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HybridCLR.HomologousImageMode.SuperSet);
            //         Debug.Log($"LoadMetadataForAOTAssembly:{i}. ret:{err}");
            //     }
            // }
            var type = assembly.GetType(name);
            if (type is null) return;
            var instance = Activator.CreateInstance(type);
            if (instance is Skill skill) skill.Init(name, src);
            else if (instance is Multi multi) multi.Init(name, src);
        }

        // public static string GetSkillType(string name)
        // {
        //     var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Skills");
        //     var instance = Activator.CreateInstance(assembly.GetType(name)) as Skill;
        //     if (instance is null) return "";
        //     return instance is Limited ? "限定技" : instance.passive ? "锁定技" : "主动技";
        // }

        public Multi parent { get; private set; } = null;

        public abstract class Multi
        {
            // public string name { get; private set; }
            public abstract List<Skill> skills { get; }
            public void Init(string name, Player src)
            {
                foreach (var i in skills)
                {
                    i.parent = this;
                    i.Init(name, src);
                }
            }
        }
    }

    /// <summary>
    /// 限定技
    /// </summary>
    public interface Limited
    {
        public bool IsDone { get; set; }
    }

    /// <summary>
    /// 持续技能 (咆哮 马术)
    /// </summary>
    public interface Durative
    {
        public void OnStart();
    }
}