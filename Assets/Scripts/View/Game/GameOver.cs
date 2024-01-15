using Spine.Unity;
using UnityEngine;

public class GameOver : SingletonMono<GameOver>
{
    public SkeletonGraphic lose;
    public SkeletonGraphic win;

    public void Show(bool isLose)
    {
        gameObject.SetActive(true);
        BGM.Instance.Stop();

        if (isLose)
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
            string scene = "Home";
            // string scene = GameCore.Room.Instance.IsSingle ? "Home" : "Lobby";
            SceneManager.Instance.LoadScene(scene);
        }
    }
}