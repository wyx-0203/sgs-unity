using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    [Serializable]
    public class Decision : Message
    {
        public int _id;
    }

    [Serializable]
    public class CardPanelQuery : Message
    {
        public int second;
        public string title;
        public string hint;
        public int dest;
        public List<int> handCards;
        public List<int> equipments;
        public List<int> judgeCards;
    }

    [Serializable]
    public class CardDecision : Decision
    {
        public List<int> cards;
    }

    [Serializable]
    public class FinishCardPanel : Message
    {
        public FinishCardPanel() { }
    }

    [Serializable]
    public class PlayQuery : Message
    {
        public SinglePlayQuery origin;
        public List<SinglePlayQuery> skills;
        public int second;
        // 可取消，即是否显示取消按钮
        public bool refusable = true;
    }

    [Serializable]
    public class SinglePlayQuery
    {
        public enum Type
        {
            Normal,
            WXKJ,
            Compete,
            PlayPhase,
            UseCard,
            LuanJi,
            SanYao
        }

        public string hint;
        public Type type = Type.Normal;

        public int maxCard;
        public int minCard;
        public IEnumerable<int> cards => destInfos.SelectMany(x => x.cards);

        public List<DestInfo> destInfos = new();

        [Serializable]
        public class DestInfo
        {
            public List<int> cards;
            public int minDest;
            public int maxDest;
            public List<int> dests;
            public List<List<int>> secondDests;
        }

        public string skillName;

        // 转换牌列表，如仁德选择一种基本牌
        public List<int> virtualCards = new();
        public List<int> disabledVirtualCards = new();
    }

    [Serializable]
    public class PlayDecision : Decision
    {
        public bool action;
        public List<int> cards = new();
        public List<int> dests = new();
        public string skill;
        // public string other;
        public int virtualCard;

        public void Clear()
        {
            action = false;
            cards.Clear();
            dests.Clear();
            skill = null;
            virtualCard = 0;
        }
    }

    [Serializable]
    public class FinishPlay : Message
    {
        public SinglePlayQuery.Type type;
    }

    [Serializable]
    public class BanQuery : Message
    {
        public int second;
    }

    [Serializable]
    public class PickQuery : Message
    {
        public int second;
    }

    [Serializable]
    public class GeneralDecision : Decision
    {
        public int general;
    }
}