using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject win;
    public GameObject lose;

    private void Start()
    {
        if (GameCore.Main.Instance.loser == GameCore.Self.Instance.team) lose.SetActive(true);
        else win.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string scene = GameCore.Room.Instance.IsSingle ? "Home" : "Lobby";
            SceneManager.Instance.LoadScene(scene);
        }
    }
}