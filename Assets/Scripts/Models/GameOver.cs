using UnityEngine.Events;

namespace Model
{
    public class GameOver : Singleton<GameOver>
    {
        private bool isOver = false;
        private bool isSurrender = false;

        public bool Loser { get; private set; }

        public void Init(bool loser)
        {
            isOver = true;
            Loser = loser;
            foreach (var p in SgsMain.Instance.AlivePlayers)
            {
                foreach (var s in p.skills) s.SetActive(false);
            }
        }

        public bool Check()
        {
            if (isOver) return true;
            if (isSurrender)
            {
                Init(Loser);
                return true;
            }
            return false;
        }

        public void Run()
        {
            GameOverView?.Invoke();
        }

        public void Surrender(bool team)
        {
            isSurrender = true;
            Loser = team;
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
                WS.Instance.SendJson(json);
            }
        }

        public UnityAction GameOverView { get; set; }
    }
}
