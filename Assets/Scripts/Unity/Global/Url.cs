using UnityEngine;

public static class Url
{
    /// <summary>
    /// 服务器域名
    /// </summary>
    public const string DOMAIN_NAME = "http://192.168.1.5:8080/";
    // public const string DOMAIN_NAME = "localhost:8080/";
    // public const string DOMAIN_NAME = "https://app931.acapp.acwing.com.cn/";
    public const string WEB_SOCKET = "wss://app931.acapp.acwing.com.cn/websocket";

    // public const string DOMAIN_NAME = "http://localhost:80/";
    // public const string WS_URL = "ws://localhost:80/websocket";

    /// <summary>
    /// Django静态文件地址
    /// </summary>
    public static string STATIC =
#if UNITY_EDITOR
        "file://" + Application.streamingAssetsPath + "/";
#else
        Application.streamingAssetsPath + "/";
#endif

    public static string ASSET_BUNDLE = STATIC + "AssetBundles/";

    public static string JSON = STATIC + "Json/";

    // 图片文件地址
    public static string IMAGE = STATIC + "Image/";
    public static string GENERAL_IMAGE = IMAGE + "General/";

    // 音频文件地址
    public static string AUDIO = STATIC + "Audio/";
}
