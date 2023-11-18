using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ABManager : GlobalSingleton<ABManager>
{
    // 依赖项配置文件
    private AssetBundleManifest manifest = null;

    // 已加载的AssetBundles字典
    public Dictionary<string, AssetBundle> ABMap { get; } = new();

    /// <summary>
    /// 从服务端获取AssetBundle并加入ABMap
    /// </summary>
    private async Task GetABFromServer(string abName)
    {
        var assetBundle = await WebRequest.GetAssetBundle(Url.ASSET_BUNDLE + abName);
        if (assetBundle is null) return;
        ABMap.Add(abName, assetBundle);
        Debug.Log("load " + abName);
    }

    /// <summary>
    /// 初始化manifest
    /// </summary>
    private async Task LoadManifest()
    {
        // 下载主包
        string mainABName = "AssetBundles";
        await GetABFromServer(mainABName);

        manifest = ABMap[mainABName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }


    /// <summary>
    /// 从外部下载AssetBundle
    /// </summary>
    public async Task LoadAssetBundle(string abName)
    {
        if (ABMap.ContainsKey(abName)) return;

        // 获取所有依赖项
        if (manifest == null) await LoadManifest();

        string[] dependencies = manifest.GetAllDependencies(abName);
        foreach (string i in dependencies)
        {
            if (!ABMap.ContainsKey(i)) await GetABFromServer(i);
        }
        // 加载目标资源包
        await GetABFromServer(abName);
    }

    // 加载进度

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    public void Unload(string abName)
    {
        if (!ABMap.ContainsKey(abName)) return;
        ABMap[abName].Unload(false);
        ABMap.Remove(abName);
    }

    public void LoadGameScene()
    {
        string url = Application.streamingAssetsPath + "/AssetBundles/";
        if (!ABManager.Instance.ABMap.ContainsKey("sprite"))
        {
            ABMap.Add("sprite", AssetBundle.LoadFromFile(url + "sprite"));
        }

        if (!ABManager.Instance.ABMap.ContainsKey("font"))
        {
            ABMap.Add("fonts", AssetBundle.LoadFromFile(url + "font"));
        }
    }
}
