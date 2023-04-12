using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Model
{
    public class AI : Singleton<AI>
    {
        private List<string> haveDamageSkill = new List<string> { "曹操", "法正", "夏侯惇", "荀彧" };
        public List<Player> DestList => SgsMain.Instance.AlivePlayers.Where(x => x.isSelf).ToList();

        // void Start()
        // {
        //     DestList = SgsMain.Instance.AlivePlayers.Where(x => x.isSelf).ToList();
        // }

        public bool SelectDest(List<Player> dests = null)
        {
            if (dests is null) dests = DestList;

            foreach (var i in dests)
            {
                if (Operation.Instance.Dests.Count == Operation.Instance.MaxDest()) break;
                if (Operation.Instance.IsValidDest(i))
                {
                    Operation.Instance.Dests.Add(i);
                }
            }

            return Operation.Instance.AICommit();

            // if (Operation.Instance.Dests.Count >= DestArea.Instance.MinDest())
            // {
            //     Operation.Instance.AICommit();
            //     Operation.Instance.Clear();
            //     return true;
            // }
            // Operation.Instance.Clear();
            // return false;
        }

        private void SortDest()
        {
            DestList.Sort((x, y) =>
            {
                if (x.Hp == 1) return -1;
                // if(y.Hp==1) return 1;
                if (haveDamageSkill.Contains(x.general.name)) return 1;
                return x.Hp < y.Hp ? -1 : 1;
            });
        }

        public bool Perform()
        {
            SortDest();
            var cardList = new List<Card>(TurnSystem.Instance.CurrentPlayer.HandCards);
            cardList.Sort((x, y) => x is 杀 ? 1 : -1);

            return cardList.Find(x => x.AIPerform()) != null;
        }
    }
}


// { "杀", typeof(杀) },
// { "闪", typeof(闪) },
// { "桃", typeof(桃) },
// { "火杀", typeof(火杀) },
// { "雷杀", typeof(雷杀) },
// { "酒", typeof(酒) },

// { "绝影", typeof(PlusHorse) },
// { "大宛", typeof(SubHorse) },
// { "赤兔", typeof(SubHorse) },
// { "爪黄飞电", typeof(PlusHorse) },
// { "的卢", typeof(PlusHorse) },
// { "紫骍", typeof(SubHorse) },
// { "骅骝", typeof(PlusHorse) },

// { "青龙偃月刀", typeof(青龙偃月刀) },
// { "麒麟弓", typeof(麒麟弓) },
// { "雌雄双股剑", typeof(雌雄双股剑) },
// { "青釭剑", typeof(青釭剑) },
// { "丈八蛇矛", typeof(丈八蛇矛) },
// { "诸葛连弩", typeof(诸葛连弩) },
// { "贯石斧", typeof(贯石斧) },
// { "方天画戟", typeof(方天画戟) },
// { "朱雀羽扇", typeof(朱雀羽扇) },
// { "古锭刀", typeof(古锭刀) },
// { "寒冰剑", typeof(寒冰剑) },

// { "八卦阵", typeof(八卦阵) },
// { "藤甲", typeof(藤甲) },
// { "仁王盾", typeof(仁王盾) },
// { "白银狮子", typeof(白银狮子) },

// { "乐不思蜀", typeof(乐不思蜀) },
// { "兵粮寸断", typeof(兵粮寸断) },
// { "闪电", typeof(闪电) },

// { "过河拆桥", typeof(过河拆桥) },
// { "顺手牵羊", typeof(顺手牵羊) },
// { "无懈可击", typeof(无懈可击) },
// { "南蛮入侵", typeof(南蛮入侵) },
// { "万箭齐发", typeof(万箭齐发) },
// { "桃园结义", typeof(桃园结义) },
// { "无中生有", typeof(无中生有) },
// { "决斗", typeof(决斗) },
// { "借刀杀人", typeof(借刀杀人) },
// { "铁索连环", typeof(铁索连环) },
// { "火攻", typeof(火攻) },