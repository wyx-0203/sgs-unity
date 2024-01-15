using System;
using Team = Model.Team;

namespace GameCore
{
    public class GameOverException : ApplicationException
    {
        public GameOverException(Team loser) => this.loser = loser;
        public Team loser;
    }

    public class CurrentPlayerDie : ApplicationException { }

    public class PlayerDie : ApplicationException { }

    public class CancelUseCard : ApplicationException { }

    public class PreventDamage : ApplicationException { }

    public class SkipPhaseException : ApplicationException { }

    public class FinishSimulation : ApplicationException { }
}
