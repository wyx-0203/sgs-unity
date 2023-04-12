using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class Sprites : GlobalSingleton<Sprites>
    {
        // 从assetbundle中加载的sprite数组
        public Sprite[] seat;
        public Sprite[] card;

        // seat

        // 装备
        public Dictionary<string, Sprite> seat_equip;
        // 花色
        public Dictionary<string, Sprite> seat_suit;
        // 黑色点数
        public Dictionary<int, Sprite> seat_blackWeight;
        // 红色点数
        public Dictionary<int, Sprite> seat_redWeight;
        // 判定牌
        public Dictionary<string, Sprite> judgeCard;

        // card
        public Dictionary<string, Sprite> cardImage;
        public Dictionary<string, Sprite> cardSuit;
        public Dictionary<int, Sprite> blackWeight;
        public Dictionary<int, Sprite> redWeight;

        public Dictionary<string, Sprite> equipImage;

        public Sprites()
        {
            // 初始化sprites
            AssetBundle assetBundle = ABManager.Instance.ABMap["sprite"];

            seat = assetBundle.LoadAssetWithSubAssets<Sprite>("seat");
            card = assetBundle.LoadAssetWithSubAssets<Sprite>("card");

            // 装备
            seat_equip = new Dictionary<string, Sprite>
            {
                {"青釭剑", seat[58]},
                {"藤甲", seat[84]},
                {"丈八蛇矛", seat[137]},
                {"大宛", seat[138]},
                {"爪黄飞电", seat[143]},
                {"的卢", seat[144]},
                {"诸葛连弩", seat[152]},
                {"方天画戟", seat[154]},
                {"朱雀羽扇", seat[157]},
                {"紫骍", seat[158]},
                {"贯石斧", seat[159]},
                {"古锭刀", seat[168]},
                {"八卦阵", seat[173]},
                {"白银狮子", seat[174]},
                {"赤兔", seat[175]},
                {"雌雄双股剑", seat[176]},
                {"寒冰剑", seat[177]},
                {"骅骝", seat[182]},
                {"绝影", seat[189]},
                {"麒麟弓", seat[191]},
                {"青龙偃月刀", seat[192]},
                {"仁王盾", seat[199]},
            };

            seat_suit = new Dictionary<string, Sprite>
            {
                {"黑桃", seat[10]},
                {"红桃", seat[221]},
                {"草花", seat[219]},
                {"方片", seat[220]},
            };

            seat_blackWeight = new Dictionary<int, Sprite>
            {
                {1, seat[293]},
                {2, seat[281]},
                {3, seat[258]},
                {4, seat[280]},
                {5, seat[279]},
                {6, seat[264]},
                {7, seat[263]},
                {8, seat[262]},
                {9, seat[261]},
                {10, seat[286]},
                {11, seat[292]},
                {12, seat[148]},
                {13, seat[295]},
            };

            seat_redWeight = new Dictionary<int, Sprite>
            {
                {1, seat[232]},
                {2, seat[259]},
                {3, seat[260]},
                {4, seat[236]},
                {5, seat[162]},
                {6, seat[170]},
                {7, seat[287]},
                {8, seat[288]},
                {9, seat[289]},
                {10, seat[161]},
                {11, seat[233]},
                {12, seat[235]},
                {13, seat[234]},
            };

            // 判定牌
            judgeCard = new Dictionary<string, Sprite>
            {
                {"乐不思蜀", seat[18]},
                {"闪电", seat[9]},
                {"兵粮寸断", seat[124]},
            };

            // card

            equipImage = new Dictionary<string, Sprite>
            {
                {"寒冰剑", card[133]},
                {"的卢", card[134]},
                {"白银狮子", card[135]},
                {"丈八蛇矛", card[140]},
                {"诸葛连弩", card[141]},
                {"紫骍", card[142]},
                {"青釭剑", card[143]},
                {"绝影", card[144]},
                {"古锭刀", card[148]},
                {"大宛", card[149]},
                {"八卦阵", card[150]},
                {"仁王盾", card[151]},
                {"麒麟弓", card[159]},
                {"骅骝", card[161]},
                {"贯石斧", card[162]},
                {"朱雀羽扇", card[163]},
                {"雌雄双股剑", card[164]},
                {"爪黄飞电", card[169]},
                {"青龙偃月刀", card[171]},
                {"方天画戟", card[184]},
                {"赤兔", card[185]},
                {"藤甲", card[188]}
            };

            cardSuit = new Dictionary<string, Sprite>
            {
                {"黑桃", card[182]},
                {"红桃", card[181]},
                {"草花", card[200]},
                {"方片", card[180]}
            };

            blackWeight = new Dictionary<int, Sprite>
            {
                {1, card[201]},
                {2, card[205]},
                {3, card[198]},
                {4, card[197]},
                {5, card[196]},
                {6, card[195]},
                {7, card[194]},
                {8, card[199]},
                {9, card[202]},
                {10, card[201]},
                {11, card[178]},
                {12, card[211]},
                {13, card[208]},
            };

            redWeight = new Dictionary<int, Sprite>
            {
                {1, card[206]},
                {2, card[177]},
                {3, card[210]},
                {4, card[176]},
                {5, card[209]},
                {6, card[175]},
                {7, card[193]},
                {8, card[203]},
                {9, card[172]},
                {10, card[179]},
                {11, card[173]},
                {12, card[174]},
                {13, card[207]},
            };

            cardImage = new Dictionary<string, Sprite>
            {
                {"青龙偃月刀", card[0]},
                {"仁王盾", card[2]},
                {"借刀杀人", card[5]},
                {"雷杀", card[6]},
                {"无中生有", card[14]},
                {"火杀", card[18]},
                {"寒冰剑", card[19]},
                {"诸葛连弩", card[23]},
                {"朱雀羽扇", card[25]},
                {"紫骍", card[26]},
                {"铁索连环", card[27]},
                {"方天画戟", card[33]},
                {"丈八蛇矛", card[37]},
                {"桃园结义", card[39]},
                {"藤甲", card[40]},
                {"麒麟弓", card[46]},
                {"贯石斧", card[48]},
                {"骅骝", card[49]},
                {"酒", card[50]},
                {"顺手牵羊", card[53]},
                {"五谷丰登", card[56]},
                {"无懈可击", card[58]},
                {"青釭剑", card[61]},
                {"古锭刀", card[63]},
                {"的卢", card[71]},
                {"大宛", card[72]},
                {"雌雄双股剑", card[74]},
                {"杀", card[77]},
                {"过河拆桥", card[78]},
                {"火攻", card[79]},
                {"决斗", card[80]},
                {"万箭齐发", card[84]},
                {"未知牌", card[85]},
                {"南蛮入侵", card[90]},
                {"闪", card[92]},
                {"绝影", card[95]},
                {"爪黄飞电", card[97]},
                {"桃", card[98]},
                {"赤兔", card[101]},
                {"兵粮寸断", card[102]},
                {"白银狮子", card[103]},
                {"闪电", card[107]},
                {"乐不思蜀", card[110]},
                {"八卦阵", card[118]},
            };
        }
    }
}