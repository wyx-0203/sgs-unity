using System;
using UnityEngine.Events;

namespace Model
{

    public class GameOverException : ApplicationException
    {
        public GameOverException(Team loser) => this.loser = loser;
        public Team loser;
    }

    public class CurrentPlayerDie : ApplicationException { }

    public class PlayerDie : ApplicationException { }

    public class PreventDamage : ApplicationException { }

    public class FinishSimulation : ApplicationException { }



    public class GameOver : Singleton<GameOver>
    {
        public Team Loser { get; private set; }

        public void Run(Team loser)
        {
            // isOver = true;
            Loser = loser;
            foreach (var p in SgsMain.Instance.AlivePlayers)
            {
                foreach (var s in p.skills) s.SetActive(false);
            }
            GameOverView?.Invoke();
        }

        public void Surrender(Team team)
        {
            throw new GameOverException(team);
        }

        // public void SendSurrender()
        // {
        //     if (Room.Instance.IsSingle) Surrender(Self.Instance.team);
        //     else
        //     {
        //         var json = new SurrenderMessage
        //         {
        //             msg_type = "surrender",
        //             team = Self.Instance.team,
        //         };
        //         WebSocket.Instance.SendMessage(json);
        //     }
        // }

        public UnityAction GameOverView { get; set; }
    }
}
