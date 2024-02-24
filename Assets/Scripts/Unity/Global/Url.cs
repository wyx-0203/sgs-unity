using UnityEngine;

public static class Url
{
    /// <summary>
    /// 服务器域名
    /// </summary>
    public const string DOMAIN_NAME = "http://192.168.1.5:8080/";
    // public const string DOMAIN_NAME = "localhost:8080/";
    // public const string DOMAIN_NAME = "https://app931.acapp.acwing.com.cn/";
    // public const string WEB_SOCKET = "wss://app931.acapp.acwing.com.cn/websocket";

    /// <summary>
    /// 静态文件地址
    /// </summary>
#if UNITY_EDITOR
    public static string STATIC = $"file://{Application.streamingAssetsPath}/";
#else
    public static string STATIC = Application.streamingAssetsPath + "/";
#endif

    public static string ASSET_BUNDLE = STATIC + "AssetBundles/";

    public static string JSON = STATIC + "Json/";

    // 图片文件地址
    public static string IMAGE = STATIC + "Image/";
    public static string GENERAL_IMAGE = IMAGE + "General/";

    // 音频文件地址
    public static string AUDIO = STATIC + "Audio/";
}
