using System;
using UnityEngine.Events;

namespace Model
{

    public class GameOverException : ApplicationException
    {
        public GameOverException(bool loser) => this.loser = loser;
        public bool loser;
    }

    public class CurrentPlayerDie : System.ApplicationException { }

    public class PlayerDie : System.ApplicationException { }

    public class PreventDamage : System.ApplicationException { }



    public class GameOver : Singleton<GameOver>
    {
        public bool Loser { get; private set; }

        public void Run(bool loser)
        {
            // isOver = true;
            Loser = loser;
            foreach (var p in SgsMain.Instance.AlivePlayers)
            {
                foreach (var s in p.skills) s.SetActive(false);
            }
            GameOverView?.Invoke();
        }

        public void Surrender(bool team)
        {
            throw new GameOverException(team);
        }

        public void SendSurrender()
        {
            if (Room.Instance.IsSingle) Surrender(Self.Instance.team);
            else
            {
                var json = new SurrenderMessage
                {
                    msg_type = "surrender",
                    team = Self.Instance.team,
                };
                WebSocket.Instance.SendMessage(json);
            }
        }

        public UnityAction GameOverView { get; set; }
    }
}
