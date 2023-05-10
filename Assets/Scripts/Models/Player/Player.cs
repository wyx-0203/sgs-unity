using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Model
{
    public class Player
    {
        public Player(bool team)
        {
            this.team = team;
        }

        public PlayerEvents events { get; private set; } = new PlayerEvents();

        public bool isSelf { get; set; } = false;
        public bool isAI { get; set; } = false;
        public Player teammate { get; set; }
        public bool team { get; private set; }

        // 武将
        public General general { get; private set; }

        // 皮肤
        public List<Skin> skins { get; private set; }
        public Skin currentSkin { get; private set; }

        // 技能
        public List<Skill> skills { get; private set; } = new List<Skill>();
        public Skill FindSkill(string name) => skills.Find(x => x.Name == name);
        // 是否存活
        public bool IsAlive { get; set; } = true;

        // 体力上限
        public int HpLimit { get; set; }
        // 体力
        public int Hp { get; set; }
        // 位置
        public int position { get; set; }
        public string posStr => (position + 1).ToString();
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
        public bool IsLocked { get; set; } = false;

        // 装备区
        public Dictionary<string, Equipage> Equipages { get; set; } = new Dictionary<string, Equipage>
        {
            { "武器", null }, { "防具", null }, { "加一马", null }, { "减一马", null }
        };
        public Weapon weapon { get => Equipages["武器"] as Weapon; }
        public Armor armor { get => Equipages["防具"] as Armor; }
        public Equipage plusHorse { get => Equipages["加一马"]; }
        public Equipage subHorse { get => Equipages["减一马"]; }

        // 判定区
        public List<DelayScheme> JudgeArea { get; set; } = new List<DelayScheme>();

        // 其他角色计算与你的距离(+1)
        public int DstPlus { get; set; } = 0;
        // 你计算与其他角色的距离(-1)
        public int DstSub { get; set; } = 0;
        // 攻击范围
        public int AttackRange { get; set; } = 1;
        // 出杀次数
        public int 杀Count { get; set; }
        public bool Use酒 { get; set; } = false;
        public int 酒Count { get; set; }

        /// <summary>
        /// 计算距离
        /// </summary>
        public int GetDistance(Player dest)
        {
            if (!dest.IsAlive || dest == this) return 0;
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

        /// <summary>
        /// 判断区域内是否有牌
        /// </summary>
        public bool RegionHaveCard => CardCount > 0 || JudgeArea.Count > 0;

        public int CardCount => HandCardCount + Equipages.Values.Where(x => x != null).Count();

        /// <summary>
        /// 按类型查找手牌(人机)
        /// </summary>
        public T FindCard<T>() where T : Card
        {
            foreach (var card in HandCards)
            {
                if (card is T && !DisabledCard(card)) return (T)card;
            }
            return null;
        }

        /// <summary>
        /// 初始化武将
        /// </summary>
        public async void InitGeneral(General general)
        {
            this.general = general;
            HpLimit = general.hp_limit;
            Hp = HpLimit;
            InitSkill();

            string url = Url.JSON + "skin/" + general.id.ToString().PadLeft(3, '0') + ".json";
            skins = JsonList<Model.Skin>.FromJson(await WebRequest.Get(url));
            currentSkin = skins[0];
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitSkill()
        {
            foreach (var str in general.skill)
            {
                if (!Skill.SkillMap.ContainsKey(str)) continue;

                var skill = (Activator.CreateInstance(Skill.SkillMap[str], this) as Skill);
                skill.Name = str;
                skills.Add(skill);
            }
        }

        /// <summary>
        /// 无次数限制
        /// </summary>
        public Func<Card, bool> unlimitedCard = (card) => false;
        public bool UnlimitedCard(Card card)
        {
            foreach (Func<Card, bool> i in unlimitedCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        public Func<Card, bool> disabledCard = (card) => false;
        public bool DisabledCard(Card card)
        {
            foreach (Func<Card, bool> i in disabledCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 卡牌对你无效
        /// </summary>
        public Func<Card, bool> disableForMe = (card) => false;
        public bool DisableForMe(Card card)
        {
            foreach (Func<Card, bool> i in disableForMe.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 无距离限制
        /// </summary>
        public Func<Card, Player, bool> unlimitedDst = (card, player) => false;
        public bool UnlimitedDst(Card card, Player dest)
        {
            foreach (Func<Card, Player, bool> i in unlimitedDst.GetInvocationList())
            {
                if (i(card, dest)) return true;
            }
            return false;
        }

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
    }
}
