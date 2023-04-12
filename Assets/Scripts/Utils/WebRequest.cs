using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;


public static class WebRequest
{
    private static async Task<bool> Send(UnityWebRequest www)
    {
        www.SendWebRequest();

        while (!www.isDone) await Task.Yield();

        if (www.result == UnityWebRequest.Result.Success) return true;

        Debug.Log(www.error);
        Debug.Log(www.url);
        return false;
    }
    public static async Task<string> Get(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        return await Send(www) ? www.downloadHandler.text : null;
    }

    public static async Task<string> GetWithToken(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", Model.Self.Instance.Token);
        return await Send(www) ? www.downloadHandler.text : null;
    }

    public static async Task<string> Post(string url, WWWForm formData)
    {
        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        return await Send(www) ? www.downloadHandler.text : null;
    }

    /// <summary>
    /// 下载纹理
    /// </summary>
    public static async Task<Texture2D> GetTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        return await Send(www) ? DownloadHandlerTexture.GetContent(www) : null;
    }

    /// <summary>
    /// 下载AudioClip
    /// </summary>
    public static async Task<AudioClip> GetClip(string url)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        return await Send(www) ? DownloadHandlerAudioClip.GetContent(www) : null;
    }

    public static async Task<AssetBundle> GetAssetBundle(string url)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        return await Send(www) ? DownloadHandlerAssetBundle.GetContent(www) : null;
    }
}
