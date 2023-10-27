using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    public class Player : IComparable<Player>
    {
        public Player(Team team, int turnOrder, bool isMaster = false)
        {
            this.team = team;
            this.turnOrder = turnOrder;
            this.isMaster = isMaster;
            effects = new EffectCollection(this);
        }

        // public PlayerEvents events { get; private set; } = new PlayerEvents();

        public bool isSelf { get; set; } = false;
        public bool isAI { get; set; } = false;
        public List<Player> teammates { get; set; }
        public Team team { get; private set; }
        public bool isMaster { get; private set; }
        public override string ToString() => general.name;

        // 武将
        public General general { get; private set; }

        // 皮肤
        public List<Skin> skins { get; private set; }
        public Skin currentSkin { get; private set; }

        // 技能
        public List<Skill> skills { get; private set; } = new List<Skill>();
        public Skill FindSkill(string name) => skills.Find(x => x.name == name);
        // 是否存活
        public bool alive { get; set; } = true;

        // 体力上限
        public int HpLimit { get; set; }
        // 体力
        public int Hp { get; set; }
        // 位置
        public int position { get; set; }
        // 回合顺序
        public int turnOrder { get; private set; }

        // 上家
        public Player last { get; set; }
        // 下家
        public Player next { get; set; }

        // 手牌
        public List<Card> HandCards { get; set; } = new List<Card>();
        // 手牌数
        public int HandCardCount => HandCards.Count;
        // 手牌上限
        public int HandCardLimit => Hp + HandCardLimitOffset;
        // 手牌上线偏移
        public int HandCardLimitOffset { get; set; } = 0;

        // 铁锁
        public bool locked { get; set; } = false;
        // 翻面
        public bool turnOver { get; set; } = false;

        // 装备区
        public Dictionary<string, Equipment> Equipments { get; set; } = new();
        public Weapon weapon => Equipments.ContainsKey("武器") ? Equipments["武器"] as Weapon : null;
        public Armor armor => Equipments.ContainsKey("防具") ? Equipments["防具"] as Armor : null;
        public PlusHorse plusHorse => Equipments.ContainsKey("加一马") ? Equipments["加一马"] as PlusHorse : null;
        public SubHorse subHorse => Equipments.ContainsKey("减一马") ? Equipments["减一马"] as SubHorse : null;

        // 判定区
        public List<DelayScheme> JudgeCards { get; set; } = new List<DelayScheme>();

        /// <summary>
        /// 所有手牌和装备牌
        /// </summary>
        public IEnumerable<Card> cards => HandCards.Union(Equipments.Values);

        // 其他角色计算与你的距离(+1)
        public int DstPlus { get; set; } = 0;
        // 你计算与其他角色的距离(-1)
        public int DstSub { get; set; } = 0;
        // 攻击范围
        public int AttackRange { get; set; } = 1;
        // 出杀次数
        public int shaCount { get; set; }
        public bool useJiu { get; set; } = false;
        public int jiuCount { get; set; }

        public EffectCollection effects { get; private set; }

        /// <summary>
        /// 计算距离
        /// </summary>
        public int GetDistance(Player dest)
        {
            if (!dest.alive || dest == this) return 0;
            int distance = 1 + dest.DstPlus - DstSub;

            Player n = next, l = last;
            while (dest != n && dest != l)
            {
                n = n.next;
                l = l.last;
                distance++;
            }

            return Mathf.Max(distance, 1);
        }

        public bool DestInAttackRange(Player dest) => dest != this && AttackRange >= GetDistance(dest);

        /// <summary>
        /// 判断区域内是否有牌
        /// </summary>
        public bool RegionIsEmpty => CardCount + JudgeCards.Count == 0;

        public int CardCount => HandCardCount + Equipments.Values.Where(x => x != null).Count();

        /// <summary>
        /// 按类型查找手牌(人机)
        /// </summary>
        public T FindCard<T>() where T : Card => HandCards.Find(x => x is T && !effects.DisableCard.Invoke(x)) as T;
        // public T FindCard<T>() where T : Card => HandCards.Find(x => x is T && !DisableCard.Instance.Invoke(this, x)) as T;

        /// <summary>
        /// 初始化武将
        /// </summary>
        public async Task InitGeneral(General general)
        {
            this.general = general;
            HpLimit = general.hp_limit;
            if (isMaster) HpLimit++;
            Hp = HpLimit;
            InitSkill();

            string url = Url.JSON + "skin/" + general.id.ToString().PadLeft(3, '0') + ".json";
            try
            {
                skins = JsonList<Model.Skin>.FromJson(await WebRequest.Get(url));
                currentSkin = skins[0];
            }
            catch (Exception e) { Util.Print(e); }
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitSkill()
        {
            foreach (var name in general.skill) Skill.New(name, this);
        }

        public int orderKey => (position - TurnSystem.Instance.CurrentPlayer.position + SgsMain.Instance.players.Length) % SgsMain.Instance.players.Length;
        public int CompareTo(Player other) => orderKey.CompareTo(other.orderKey);

        /// <summary>
        /// 无次数限制
        /// </summary>
        // public Func<Card, bool> unlimitTimes = (card) => false;
        // public bool UnlimitTimes(Card card)
        // {
        //     foreach (Func<Card, bool> i in unlimitTimes.GetInvocationList())
        //     {
        //         if (i(card)) return true;
        //     }
        //     return false;
        // }

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        // public Func<Card, bool> disabledCard = (card) => false;
        // public bool DisabledCard(Card card)
        // {
        //     foreach (Func<Card, bool> i in disabledCard.GetInvocationList())
        //     {
        //         if (i(card)) return true;
        //     }
        //     return false;
        // }

        /// <summary>
        /// 卡牌对你无效
        /// </summary>
        // public Func<Card, bool> disableForMe = (card) => false;
        // public bool DisableForMe(Card card)
        // {
        //     foreach (Func<Card, bool> i in disableForMe.GetInvocationList())
        //     {
        //         if (i(card)) return true;
        //     }
        //     return false;
        // }

        /// <summary>
        /// 无距离限制
        /// </summary>
        // public Func<Card, Player, bool> unlimitDst = (card, player) => false;
        // public bool UnlimitDst(Card card, Player dest)
        // {
        //     foreach (Func<Card, Player, bool> i in unlimitDst.GetInvocationList())
        //     {
        //         if (i(card, dest)) return true;
        //     }
        //     return false;
        // }

        private int skinIndex = 0;
        public void SendChangeSkin()
        {
            skinIndex = (skinIndex + 1) % skins.Count;
            var json = new ChangeSkinMessage
            {
                msg_type = "change_skin",
                position = position,
                skin_id = skins[skinIndex].id
            };

            if (Room.Instance.IsSingle) ChangeSkin(json.skin_id);
            else WebSocket.Instance.SendMessage(json);
        }

        public void ChangeSkin(int skinId)
        {
            currentSkin = skins.Find(x => x.id == skinId);
            ChangeSkinView?.Invoke(skinId.ToString(), currentSkin.name);
        }

        public UnityAction<string, string> ChangeSkinView { get; set; }

        public string DebugInfo()
        {
            string str = ToString();
            str += "\nhp:" + Hp;
            str += "\nhandcards:" + string.Join(' ', HandCards);
            str += "\nequipments:" + string.Join(' ', Equipments.Values);
            str += "\njudges:" + string.Join(' ', JudgeCards) + '\n';
            return str;
        }
    }
}
