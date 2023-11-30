using Spine.Unity;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public SkeletonGraphic lose;
    public SkeletonGraphic win;

    private void Start()
    {
        BGM.Instance.Stop();

        if (GameCore.Main.Instance.loser == GameCore.Self.Instance.team)
        {
            lose.AnimationState.AddAnimation(0, "play2", true, 0);
            lose.gameObject.SetActive(true);
        }
        else
        {
            win.AnimationState.AddAnimation(0, "play2", true, 0);
            win.gameObject.SetActive(true);
        }
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