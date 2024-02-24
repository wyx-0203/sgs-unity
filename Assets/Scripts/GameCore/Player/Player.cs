using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team = Model.Team;
using General = Model.General;

namespace GameCore
{
    public class Player : IComparable<Player>
    {
        public Player(Team team, int turnOrder, bool isMaster = false)
        {
            this.team = team;
            this.turnOrder = turnOrder;
            this.isMonarch = isMaster;
            effects = new EffectCollection(this);
        }

        public Game game { get; set; }

        /// <summary>
        /// 是否为AI
        /// </summary>
        public bool isAI { get; set; } = false;

        /// <summary>
        /// 所有队友
        /// </summary>
        public List<Player> teammates { get; set; }

        /// <summary>
        /// 阵营
        /// </summary>
        public Team team { get; private set; }

        /// <summary>
        /// 是否为主将
        /// </summary>
        public bool isMonarch { get; private set; }

        public override string ToString() => general?.name;

        /// <summary>
        /// 武将
        /// </summary>
        public General general { get; private set; }

        /// <summary>
        /// 所有技能
        /// </summary>
        public List<Skill> skills { get; } = new();

        /// <summary>
        /// 按名称查找技能
        /// </summary>
        public Skill FindSkill(string name) => skills.Find(x => x.name == name);

        /// <summary>
        /// 按类型查找技能
        /// </summary>
        public T FindSkill<T>() where T : Skill => skills.Find(x => x is T) as T;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool alive { get; set; } = true;

        /// <summary>
        /// 体力上限
        /// </summary>
        public int hpLimit { get; set; }

        /// <summary>
        /// 体力
        /// </summary>
        public int hp { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public int position { get; set; }

        /// <summary>
        /// 回合顺序
        /// </summary>
        public int turnOrder { get; private set; }

        /// <summary>
        /// 上家
        /// </summary>
        public Player last { get; set; }

        /// <summary>
        /// 下家
        /// </summary>
        public Player next { get; set; }

        /// <summary>
        /// 手牌
        /// </summary>
        public List<Card> handCards { get; } = new();

        /// <summary>
        /// 手牌数
        /// </summary>
        public int handCardsCount => handCards.Count;

        /// <summary>
        /// 手牌上限
        /// </summary>
        public int handCardsLimit => hp + HandCardLimitOffset;

        /// <summary>
        /// 手牌上限偏移
        /// </summary>
        public int HandCardLimitOffset { get; set; } = 0;

        /// <summary>
        /// 横置
        /// </summary>
        public bool locked { get; set; } = false;

        /// <summary>
        /// 翻面
        /// </summary>
        public bool isTurnOver { get; set; } = false;

        /// <summary>
        /// 装备区
        /// </summary>
        public Dictionary<string, Equipment> Equipments { get; } = new();

        /// <summary>
        /// 武器
        /// </summary>
        public Weapon weapon => Equipments.ContainsKey("武器") ? Equipments["武器"] as Weapon : null;

        /// <summary>
        /// 防具
        /// </summary>
        public Armor armor => Equipments.ContainsKey("防具") ? Equipments["防具"] as Armor : null;

        /// <summary>
        /// 加一马
        /// </summary>
        public PlusHorse plusHorse => Equipments.ContainsKey("加一马") ? Equipments["加一马"] as PlusHorse : null;

        /// <summary>
        /// 减一马
        /// </summary>
        public SubHorse subHorse => Equipments.ContainsKey("减一马") ? Equipments["减一马"] as SubHorse : null;

        /// <summary>
        /// 判定区
        /// </summary>
        public List<DelayScheme> JudgeCards { get; } = new();

        /// <summary>
        /// 所有手牌和装备牌
        /// </summary>
        public IEnumerable<Card> cards => handCards.Union(Equipments.Values);

        /// <summary>
        /// 其他角色计算与你的距离(+1)
        /// </summary>
        public int plusDst { get; set; } = 0;

        /// <summary>
        /// 你计算与其他角色的距离(-1)
        /// </summary>
        public int subDst { get; set; } = 0;

        /// <summary>
        /// 攻击范围
        /// </summary>
        public int attackRange { get; set; } = 1;

        /// <summary>
        /// 出杀次数
        /// </summary>
        public int shaCount { get; set; } = 0;

        public bool useJiu { get; set; } = false;
        public int jiuCount { get; set; }

        public EffectCollection effects { get; private set; }

        /// <summary>
        /// 计算距离
        /// </summary>
        public int GetDistance(Player dest)
        {
            if (!dest.alive || dest == this) return 0;
            int distance = 1 + dest.plusDst - subDst;

            Player n = next, l = last;
            while (dest != n && dest != l)
            {
                n = n.next;
                l = l.last;
                distance++;
            }

            return Math.Max(distance, 1);
        }

        /// <summary>
        /// 目标是否在你的攻击范围内
        /// </summary>
        public bool DestInAttackRange(Player dest) => dest != this && attackRange >= GetDistance(dest);

        /// <summary>
        /// 区域内是否为空
        /// </summary>
        public bool regionIsEmpty => cardsCount + JudgeCards.Count == 0;

        /// <summary>
        /// 手牌+装备牌的数量
        /// </summary>
        public int cardsCount => cards.Count();

        /// <summary>
        /// 按类型查找手牌(人机)
        /// </summary>
        public T FindCard<T>() where T : Card => handCards.Find(x => x is T && x.useable) as T;

        /// <summary>
        /// 初始化武将
        /// </summary>
        public void InitGeneral(General general)
        {
            // 基本信息
            this.general = general;
            hpLimit = general.hpLimit;
            if (isMonarch) hpLimit++;
            hp = hpLimit;

            // 添加技能
            foreach (var name in general.skills) Skill.New(name, this);

            // Debug.Log("game1");
            game.eventSystem.SendToClient(new Model.InitGeneral
            {
                player = position,
                general = general.id,
                hp = hp,
                hpLimit = hpLimit,
                skills = skills.Select(x => x.name).Distinct().ToList()
            });
            // await Task.Yield();
            // Debug.Log("game2");
            // 皮肤
            // skins = (await Skin.GetList(general.id)).ToList();
            // currentSkin = skins[0];
        }

        // public List<Model.SkillAsset> GetSkillModels()
        // {
        //     var list = new List<Model.SkillAsset>();
        //     foreach (var i in skills) if (list.Find(x => x.name == i.name) is null) list.Add(i.ToModel());
        //     return list;
        // }

        /// <summary>
        /// 按当前回合角色排序
        /// </summary>
        public int orderKey => (position - game.turnSystem.CurrentPlayer.position + game.players.Length) % game.players.Length;

        /// <summary>
        /// 按当前回合角色排序
        /// </summary>
        public int CompareTo(Player other) => orderKey.CompareTo(other.orderKey);

        #region 皮肤

        /// <summary>
        /// 皮肤
        /// </summary>
        // public List<Skin> skins { get; private set; }

        /// <summary>
        /// 当前皮肤
        /// </summary>
        // public Skin currentSkin { get; private set; }

        // private int skinIndex = 0;
        // public void SendChangeSkin()
        // {
        //     skinIndex = (skinIndex + 1) % skins.Count;
        //     var json = new Model.ChangeSkinMessage
        //     {
        //         type = "change_skin",
        //         position = position,
        //         skin_id = skins[skinIndex].id
        //     };

        //     if (Room.Instance.IsSingle) ChangeSkin(json.skin_id);
        //     else WebSocket.Instance.SendMessage(json);
        // }

        // public void ChangeSkin(int skinId)
        // {
        //     currentSkin = skins.Find(x => x.id == skinId);
        //     ChangeSkinView?.Invoke(currentSkin);
        // }

        // public Action<Skin> ChangeSkinView;

        #endregion

        public string DebugInfo()
        {
            string str = ToString();
            str += "\nhp:" + hp;
            str += "\nhandcards:" + string.Join(' ', handCards);
            str += "\nequipments:" + string.Join(' ', Equipments.Values);
            str += "\njudges:" + string.Join(' ', JudgeCards) + '\n';
            return str;
        }
    }
}
