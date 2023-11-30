using System;
using System.Collections.Generic;

namespace Models
{
    public class Buff
    {
    }


    public enum TimerType
    {
        Normal,
        WXKJ,
        Compete,
        InPlayPhase
    }

    public enum DestAmount
    {
        None,
        UseCard
    }

    public enum CardCondition
    {
        NotConverted,
        UseCard,
    }

    public enum DestCondition
    {
        Given,
        UseCard,
        // 
    }

    public enum NoTimesLimit
    {
        // 
    }

    public enum NoDistanceLimit
    {
        // 
    }

    public enum DisableCard
    {
        // 
    }

    public enum ExtraDestAmount
    {
        // 
    }

    [Serializable]
    public class TimerJson
    {
        public int player;
        public int second;
        public string hint;
        public TimerType type = TimerType.Normal;
        // 可取消，即是否显示取消按钮
        public bool refusable = true;

        public int maxCard;
        public int minCard;
        public int maxDest;
        public int minDest;
        public CardCondition cardCond;
        public DestCondition destCond;

        // public NoTimesLimit noTimesLimit;
        // public NoDistanceLimit noDistanceLimit;
        // public DisableCard disableCard;
        // public ExtraDestAmount extraDestAmount;

        public List<SkillTimerJson> skills;

        // 转换牌列表，如仁德选择一种基本牌
        public List<int> multiConverted = new();

        public string equipSkill;
    }

    [Serializable]
    public class SkillTimerJson
    {
        public string name;
        public int maxCard;
        public int minCard;
        public int maxDest;
        public int minDest;
        public CardCondition cardCond;
        public DestCondition destCond;
    }
}