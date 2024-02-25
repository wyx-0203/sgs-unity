using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ABManager : Singleton<ABManager>
{
    // 依赖项配置文件
    private AssetBundleManifest manifest = null;

    // 已加载的AssetBundles字典
    private Dictionary<string, AssetBundle> abMap { get; } = new();

    /// <summary>
    /// 从服务端获取AssetBundle并加入ABMap
    /// </summary>
    private async Task GetABFromServer(string abName)
    {
        var assetBundle = await WebRequest.GetAssetBundle(Url.ASSET_BUNDLE + abName);
        if (assetBundle is null) return;
        abMap.Add(abName, assetBundle);
        Debug.Log("load " + abName);
    }

    /// <summary>
    /// 初始化manifest
    /// </summary>
    public async Task LoadManifest()
    {
        // 下载主包
        string mainABName = "AssetBundles";
        await GetABFromServer(mainABName);

        manifest = abMap[mainABName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    /// <summary>
    /// 从外部下载AssetBundle
    /// </summary>
    public async Task<AssetBundle> Load(string abName)
    {
        await semaphoreSlim.WaitAsync();
        if (!abMap.ContainsKey(abName))
        {
            // if (manifest == null) await LoadManifest();

            // 获取所有依赖项
            string[] dependencies = manifest.GetAllDependencies(abName);
            foreach (string i in dependencies.Where(x => !abMap.ContainsKey(x))) await GetABFromServer(i);

            // 加载目标资源包
            await GetABFromServer(abName);
        }

        semaphoreSlim.Release();
        return abMap[abName];
    }

    private readonly SemaphoreSlim semaphoreSlim = new(1);

    // 加载进度

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    public void Unload(string abName)
    {
        if (!abMap.ContainsKey(abName)) return;
        abMap[abName].Unload(false);
        abMap.Remove(abName);
    }

    public async Task LoadGameScene()
    {
        await Load("sprite");
        await Load("spine-base");
    }
}
