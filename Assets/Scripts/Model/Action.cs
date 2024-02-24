using System;
using System.Collections.Generic;

namespace Model
{
    [Serializable]
    public abstract class UpdateCard : Message
    {
        public List<int> cards;
        public int handCardsCount;
    }

    [Serializable]
    public class GetCard : UpdateCard { }

    [Serializable]
    public class DrawCard : GetCard
    {
        public int pileCount;
    }

    [Serializable]
    public class GetDiscard : GetCard { }

    [Serializable]
    public class GetCardInJudgeArea : GetCard
    {
        public int dest;
    }

    [Serializable]
    public class LoseCard : UpdateCard { }

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
    public class Recover : UpdateHp { }

    [Serializable]
    public class LoseHp : UpdateHp { }

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
}
