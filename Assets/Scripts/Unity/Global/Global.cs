using UnityEngine;

public class Global : Singleton<Global>
{
    public string token { get; set; }
    public int userId { get; set; } = Model.User.StandaloneId;
    public int roomId { get; set; } = -1;

    public bool IsStandalone => userId == Model.User.StandaloneId;


    [RuntimeInitializeOnLoadMethod]
    public static async void Init()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

        await ABManager.Instance.LoadManifest();
        await GlobalAsset.Init();
        await GeneralsAsset.Init();
        Model.General.Init(await WebRequest.Get(Url.JSON + "general.json"));
        Model.Card.Init(await WebRequest.Get(Url.JSON + "card.json"));
        await SkinAsset.Init();
        await SkillAsset.Init();

        Debug.Log("after init");
    }
}
