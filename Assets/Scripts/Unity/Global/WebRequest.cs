using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


public static class WebRequest
{
    public static TaskAwaiter<object> GetAwaiter(this UnityWebRequestAsyncOperation op)
    {
        var tcs = new TaskCompletionSource<object>();
        op.completed += x => tcs.SetResult(null);
        return tcs.Task.GetAwaiter();
    }

    public static async Task<string> Get(string url)
    {
        using var www = UnityWebRequest.Get(url);
        await www.SendWebRequest();
        return www.downloadHandler.text;
    }

    public static async Task<string> GetWithToken(string url)
    {
        using var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", Global.Instance.token);
        await www.SendWebRequest();
        return www.downloadHandler.text;
    }

    public static async Task<string> Post(string url, WWWForm formData)
    {
        using var www = UnityWebRequest.Post(url, formData);
        await www.SendWebRequest();
        return www.downloadHandler.text;
    }

    /// <summary>
    /// 下载纹理
    /// </summary>
    public static async Task<Texture2D> GetTexture(string url)
    {
        using var www = UnityWebRequestTexture.GetTexture(url);
        await www.SendWebRequest();
        return DownloadHandlerTexture.GetContent(www);
    }

    public static async Task<AssetBundle> GetAssetBundle(string url)
    {
        using var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        await www.SendWebRequest();
        return DownloadHandlerAssetBundle.GetContent(www);
    }
}
