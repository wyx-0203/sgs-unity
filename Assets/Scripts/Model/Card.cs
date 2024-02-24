using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Model
{
    [Serializable]
    public class Card
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int id;
        /// <summary>
        /// 花色
        /// </summary>
        public string suit;
        /// <summary>
        /// 点数(1~13)
        /// </summary>
        public int weight;
        /// <summary>
        /// 类别
        /// </summary>
        public string type;
        /// <summary>
        /// 卡牌名
        /// </summary>
        public string name;

        #region 转化牌

        /// <summary>
        /// 是否为转化牌
        /// </summary>
        // public bool isConvert { get; private set; } = false;

        /// <summary>
        /// 转化牌的原型
        /// </summary>
        // public List<Card> PrimiTives { get; } = new();

        [JsonIgnore]
        public bool isVirtual => id < 0;
        private static Card NewVirtual(int id)
        {
            if (virtualList.FirstOrDefault(x => x.id == id) is Card card) return card;
            card = new Card
            {
                id = id,
                name = _cards[-id].name,
                type = _cards[-id].type
            };
            virtualList.Add(card);
            return card;
        }

        private static List<Card> _cards;
        private static List<Card> virtualList = new();
        public static void Init(string json)
        {
            if (_cards is null) _cards = JsonConvert.DeserializeObject<List<Card>>(json);
        }
        public static List<Card> GetList() => _cards;
        public static Card Find(int id) => id > 0 ? _cards[id] : NewVirtual(id);
        #endregion

        [JsonIgnore]
        public bool isRed => suit == "红桃" || suit == "方片" || suit == "红色";
        [JsonIgnore]
        public bool isBlack => suit == "黑桃" || suit == "草花" || suit == "黑色";

        public override string ToString()
        {
            string symbol;
            switch (suit)
            {
                case "红桃": symbol = "♥️"; break;
                case "方片": symbol = "♦️"; break;
                case "黑桃": symbol = "♠️"; break;
                case "草花": symbol = "♣️"; break;
                default: symbol = ""; break;
            }
            return "【" + name + symbol + weight + "】";
        }
    }

    public class UseCard : Message
    {
        public List<int> dests;
        public int id = -1;
        public string name;
        public string type;
        public Gender gender;
    }
}