using System;
using System.Collections.Generic;

namespace Model
{
    public enum Team
    {
        None = 0, BLUE = 1, RED = -2
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

    public enum Mode
    {
        _2V2,
        _3V3
    }


    [Serializable]
    public class InitPlayer : Message
    {
        public List<int> id;
        public Team GetTeam(int userId) => id.IndexOf(userId) == 0 ? Team.BLUE : Team.RED;
        public List<Team> team;
        public List<int> position;
        public List<bool> isMonarch;
    }

    [Serializable]
    public class InitGeneral : Message
    {
        public int general;
        public int hpLimit;
        public int hp;
        public List<string> skills;
    }

    [Serializable]
    public class GameOver : Message
    {
        public Team loser;
    }

    [Serializable]
    public class Surrender : Message
    {
        public Team team;
    }

    public class NewRound : Message
    {
        public int round;
    }

    [Serializable]
    public class StartBanPick : Message
    {
        public List<int> generals;
    }

    [Serializable]
    public class FinishBanPick : Message
    {
        public DateTime startFightTime;
    }

    [Serializable]
    public class OnBan : Message
    {
        public int general;
    }

    [Serializable]
    public class OnPick : Message
    {
        public int general;
    }

    [Serializable]
    public class StartSelfPick : Message
    {
        public int second;
    }

    [Serializable]
    public class Shuffle : Decision
    {
        public List<int> cards;
    }

    [Serializable]
    public class AddToDiscard : Message
    {
        public List<int> cards;
    }


    [Serializable] public class StartTurn : Message { }
    [Serializable] public class FinishTurn : Message { }
    [Serializable]
    public class StartPhase : Message
    {
        public Phase phase;
    }
    [Serializable] public class FinishPhase : Message { }
    [Serializable] public class FinishOncePlay : Message { }


    [Serializable]
    public class ChangeSkin : Message
    {
        public int skinId;
    }
}
