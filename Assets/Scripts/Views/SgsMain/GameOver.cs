using UnityEngine;

namespace View
{
    public class GameOver : MonoBehaviour
    {
        public GameObject win;
        public GameObject lose;

        private void Start()
        {
            if (Model.SgsMain.Instance.loser == Model.Self.Instance.team) lose.SetActive(true);
            else win.SetActive(true);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                string scene = Model.Room.Instance.IsSingle ? "Home" : "Lobby";
                SceneManager.Instance.LoadScene(scene);
            }
        }
    }
}
