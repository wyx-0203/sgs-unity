namespace Model
{
    public class DestArea : Singleton<DestArea>
    {
        // private Decision Timer.Instance.temp => Decision.temp;
        private Card card => Timer.Instance.temp.converted is null ? Timer.Instance.temp.cards[0] : Timer.Instance.temp.converted;

        /// <summary>
        /// 根据玩家和卡牌id初始化目标数量
        /// </summary>
        /// <returns>目标数量最大值与最小值</returns>
        public int MaxDest()
        {
            // var player = Timer.Instance.players[0];

            switch (card.name)
            {
                case "杀":
                case "雷杀":
                case "火杀":
                case "决斗":
                case "过河拆桥":
                case "顺手牵羊":
                case "乐不思蜀":
                case "兵粮寸断":
                case "火攻":
                case "借刀杀人":
                    return 1;
                case "铁索连环":
                    return 2;
                default:
                    return 0;
            }
        }

        public int MinDest()
        {

            switch (card.name)
            {
                case "杀":
                case "雷杀":
                case "火杀":
                case "决斗":
                case "过河拆桥":
                case "顺手牵羊":
                case "乐不思蜀":
                case "兵粮寸断":
                case "火攻":
                case "借刀杀人":
                    return 1;
                default:
                    return 0;
            }
        }

        public int ShaMaxDest(Player player)
        {
            int maxCount = 1;
            if (player.HandCardCount == 1 && player.Equipments["武器"] is 方天画戟) maxCount += 2;
            return maxCount;
        }

        /// <summary>
        /// 判断dest是否能成为src的目标
        /// </summary>
        public bool ValidDest(Player dest)
        {
            var src = TurnSystem.Instance.CurrentPlayer;
            if (!dest.IsAlive) return false;
            if (src != dest && src.UnlimitDst(card, dest)) return true;

            switch (card.name)
            {
                case "杀":
                case "火杀":
                case "雷杀":
                    return UseSha(src, dest);

                case "酒":
                    return src == dest;

                case "过河拆桥":
                    return src != dest && !dest.RegionIsEmpty;

                case "顺手牵羊":
                    return src.GetDistance(dest) == 1 && !dest.RegionIsEmpty;

                case "借刀杀人":
                    if (dest.weapon is null || src == dest) return false;
                    foreach (var i in SgsMain.Instance.players)
                    {
                        if (UseSha(dest, i)) return true;
                    }
                    return false;
                // if (operation.dests.Count == 0) return src != dest && dest.weapon != null;
                // else return UseSha(operation.dests[0], dest);

                case "决斗":
                    return src != dest;

                case "火攻":
                    return dest.HandCardCount != 0;

                case "兵粮寸断":
                    return src.GetDistance(dest) == 1 && dest.JudgeCards.Find(x => x is 兵粮寸断) is null;

                case "乐不思蜀":
                    return src != dest && dest.JudgeCards.Find(x => x is 乐不思蜀) is null;

                default:
                    return true;
            }
        }

        public bool UseSha(Player src, Player dest)
        {
            return src != dest && src.AttackRange >= src.GetDistance(dest);
        }
    }
}
