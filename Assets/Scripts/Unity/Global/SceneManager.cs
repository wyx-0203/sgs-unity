using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    private bool isDone = true;
    private const string localScene = "Home";

    public async void LoadScene(string sceneName)
    {
        // 若场景正在加载，则直接返回
        if (!isDone) return;
        isDone = false;

        GameObject.Instantiate(GlobalAsset.Instance.loading, Transform.FindObjectOfType<Canvas>().transform);

        // 卸载当前场景的AssetBundle
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != localScene)
        {
            ABManager.Instance.Unload(currentScene.ToLower());
            Debug.Log("unload " + currentScene);
        }

        // 若新场景为本地场景(主页)，则直接加载场景，不需加载AssetBundle
        if (sceneName == localScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            // 加载包含场景资源的AssetBundle
            string abName = sceneName.ToLower();
            await ABManager.Instance.Load(abName);
            // 获取场景路径
            string[] scenePath = (await ABManager.Instance.Load(abName)).GetAllScenePaths();
            // 加载场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath[0]);
        }
        isDone = true;
    }
}