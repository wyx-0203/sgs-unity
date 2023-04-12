using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : GlobalSingleton<SceneManager>
{
    private bool isDone = true;
    private GameObject loading;

    /// <summary>
    /// 场景名与AB包名的映射
    /// </summary>
    private Dictionary<string, string> sceneMap = new Dictionary<string, string>
    {
        { "Lobby", "lobby" },
        { "Room", "room" },
        { "SgsMain", "sgsmain" },
    };

    private string localScene = "Home";

    public async void LoadScene(string sceneName)
    {
        // 若场景正在加载，则直接返回
        if (!isDone) return;

        if (loading is null)
        {
            await ABManager.Instance.LoadAssetBundle("loading");
            loading = ABManager.Instance.ABMap["loading"].LoadAsset<GameObject>("Loading");
        }
        GameObject.Instantiate(loading, GameObject.Find("Canvas").transform);

        // 卸载当前场景的AssetBundle
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != localScene)
        {
            string currentAB = sceneMap[currentScene];
            ABManager.Instance.Unload(currentAB);
            // debug
            Debug.Log("unload " + currentAB);
        }

        // 若新场景为本地场景(主页)，则直接加载场景，不需加载AssetBundle
        if (sceneName == localScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            return;
        }

        isDone = false;
        // 加载包含场景资源的AssetBundle
        string abName = sceneMap[sceneName];
        await ABManager.Instance.LoadAssetBundle(abName);
        // 获取场景路径
        string[] scenePath = ABManager.Instance.ABMap[abName].GetAllScenePaths();
        // 加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath[0]);
        isDone = true;

    }
}