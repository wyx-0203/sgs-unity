using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using System.Text.Json;

namespace Model
{
    public class Player
    {
        /// <summary>
        /// 是否为自己
        /// </summary>
        public bool isSelf { get; set; }

        /// <summary>
        /// 阵营
        /// </summary>
        public Team team { get; set; }

        /// <summary>
        /// 是否为主将
        /// </summary>
        public bool isMonarch { get; set; }

        /// <summary>
        /// 武将
        /// </summary>
        public General general { get; set; }

        /// <summary>
        /// 所有技能
        /// </summary>
        public List<string> skills { get; set; }

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
        public int index { get; set; }

        /// <summary>
        /// 回合顺序
        /// </summary>
        public int turnOrder { get; set; }

        /// <summary>
        /// 手牌
        /// </summary>
        public List<Card> handCards { get; } = new();

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
        public Dictionary<string, Card> Equipments { get; } = new();

        /// <summary>
        /// 判定区
        /// </summary>
        public List<Card> JudgeCards { get; } = new();

        /// <summary>
        /// 所有手牌和装备牌
        /// </summary>
        public IEnumerable<Card> cards => handCards.Union(Equipments.Values);

        // public List<int> skins;
        public int currentSkin;
    }
}
