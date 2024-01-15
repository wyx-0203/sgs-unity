using System;
using System.Collections.Generic;

namespace Model
{
    [Serializable]
    public class Message
    {
        public string _type;
        public int player = -1;

        public Message()
        {
            _type = GetType().ToString();
        }
    }

    [Serializable]
    public abstract class UpdateCard : Message
    {
        public List<int> cards;
        public int handCardsCount;
    }

    [Serializable]
    public abstract class GetCard : UpdateCard { public GetCard() : base() { } }

    [Serializable]
    public class DrawCard : GetCard { public DrawCard() : base() { } }

    [Serializable]
    public class GetDiscard : GetCard { public GetDiscard() : base() { } }

    [Serializable]
    public class GetCardInJudgeArea : GetCard
    {
        public int dest;
    }

    [Serializable]
    public class LoseCard : UpdateCard { public LoseCard() : base() { } }

    [Serializable]
    public class ShowCard : Message
    {
        public List<int> cards;
    }

    [Serializable]
    public class GetAnothersCard : GetCard
    {
        public int dest;
        public int destCount;
        public List<bool> known;
    }

    [Serializable]
    public class Die : Message
    {
        public int damageSrc;
    }

    [Serializable]
    public abstract class UpdateHp : Message
    {
        public int value;
        public int hp;
        public int handCardsLimit;
    }

    [Serializable]
    public class Recover : UpdateHp { public Recover() : base() { } }

    [Serializable]
    public class LoseHp : UpdateHp { public LoseHp() : base() { } }

    [Serializable]
    public class Damage : UpdateHp
    {
        public enum Type
        {
            Normal,
            Fire,
            Thunder
        }
        public Type type;
        public int src;
    }

    [Serializable]
    public class SetLock : Message
    {
        public bool value;
        public bool byDamage;
    }

    [Serializable]
    public class TurnOver : Message
    {
        public bool value;
    }

    [Serializable]
    public class Skill
    {
        public enum Type
        {
            Normal,
            // Active,
            // Converted,
            Passive,
            Limited,
            Awoken,
            Monarch
        }
        public string name;
        public Type type;
        public string describe;
    }

    [Serializable]
    public class UpdateSkill : Message
    {
        public List<Skill> skills;
    }

    [Serializable]
    public class UseSkill : Message
    {
        public List<int> dests;
        public string name;
    }

    // [Serializable]
    // public class RemoveSkill : Message
    // {
    //     public List<string> skills;
    //     // public string src;
    // }

    [Serializable]
    public class AddEquipment : Message
    {
        public int card;
    }

    [Serializable]
    public class AddJudgeCard : Message
    {
        public int card;
    }

    [Serializable]
    public class RemoveJudgeCard : Message
    {
        public int card;
    }

    [Serializable]
    public class AddToDiscard : Message
    {
        public List<int> cards;
    }

    [Serializable]
    public class UpdatePileCount : Message
    {
        public int count;
    }


    public enum Phase
    {
        Prepare,    // 准备阶段
        Judge,      // 判定阶段
        Get,        // 摸牌阶段
        Play,       // 出牌阶段
        Discard,    // 弃牌阶段
        End,        // 结束阶段
    }

    [Serializable] public class StartTurn : Message { public StartTurn() : base() { } }
    [Serializable] public class FinishTurn : Message { public FinishTurn() : base() { } }
    [Serializable]
    public class StartPhase : Message
    {
        public Phase phase;
    }
    [Serializable] public class FinishPhase : Message { public FinishPhase() : base() { } }
    [Serializable] public class FinishOncePlay : Message { public FinishOncePlay() : base() { } }


    [Serializable]
    public class ChangeSkin : Message
    {
        public int skinId;
    }


    [Serializable]
    public class InitPlayer : Message
    {
        public int[] id;
        public List<Team> team;
        public List<int> position;
        public List<bool> isMonarch;
    }

    // [Serializable] public class StartBanPick : Message { public StartBanPick() : base() { } }

    [Serializable]
    public class InitGeneral : Message
    {
        public int general;
        public int hpLimit;
        public int hp;
        public List<Skill> skills;
    }

    [Serializable]
    public class GameOver : Message
    {
        public Team loser;
    }
}
