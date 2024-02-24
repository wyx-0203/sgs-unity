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
            lose.gameObject.SetActive(true);
            lose.AnimationState.AddAnimation(0, "play2", true, 0);
        }
        else
        {
            win.gameObject.SetActive(true);
            win.AnimationState.AddAnimation(0, "play2", true, 0);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string scene = Global.Instance.IsStandalone ? "Home" : "Lobby";
            SceneManager.Instance.LoadScene(scene);
        }
    }
}