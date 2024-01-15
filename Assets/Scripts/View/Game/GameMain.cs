using Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameMain : SingletonMono<GameMain>
{
    public GameAssets gameAssets;
    public Image background;

    public List<Player> players { get; } = new();

    /// <summary>
    /// 主视角玩家
    /// </summary>
    public Player firstPerson;
    public RectTransform border;

    public GameObject playerPrefab;
    public Transform _selfs;
    public Transform _enemys;

    public Team selfTeam;


    private async void Start()
    {
#if UNITY_EDITOR
        await ABManager.Instance.LoadGameScene();
#elif UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

        SetBorder();
        bgIndex = Random.Range(0, gameAssets.background.Count);
        ChangeBg();
        BGM.Instance.Load(gameAssets.bgm);

        EventSystem.Instance.AddEvent<CardPanelQuery>(ShowPanel);
        EventSystem.Instance.AddEvent<FinishCardPanel>(HidePanel);

        EventSystem.Instance.AddEvent<InitPlayer>(Init);
        EventSystem.Instance.AddEvent<StartBanPick>(OnStartBP);
        EventSystem.Instance.AddEvent<FinishBanPick>(OnFinishBP);
        EventSystem.Instance.AddEvent<Model.GameOver>(OnGameOver);

        EventSystem.Instance.AddEvent<Die>(OnPlayerDead);

        EventSystem.Instance.AddPriorEvent<StartTurn>(ChangeView);
        EventSystem.Instance.AddPriorEvent<PlayQuery>(ChangeView);

        // var a = ScriptableObject.CreateInstance<GameAssets>();
        // var cards = await Model.Card.GetList();
        // System.Func<string, int> order = x =>
        // {
        //     var c = cards.Find(y => y.name == x);
        //     if (c is null) return 1000;
        //     return c.id + (c.type == "基本牌" ? 0 : c.type == "锦囊牌" ? 200 : c.type == "延时锦囊" ? 400 : 600);
        // };
        // a.cardImage = KeyValueList.New(Sprites.Instance.cardImage);
        // a.cardImage.Sort((x, y) => order(x.key).CompareTo(order(y.key)));

        // var names = Sprites.Instance.cardImage.Keys.OrderBy(order);
        // a.cardMaleSound = names.Select(x => new KeyValue<AudioClip>
        // {
        //     key = x,
        //     value = Resources.Load<AudioClip>(Audio.Instance.GetMalePath(x, cards))
        // }).ToList();
        // a.cardFemaleSound = names.Select(x => new KeyValue<AudioClip>
        // {
        //     key = x,
        //     value = Resources.Load<AudioClip>(Audio.Instance.GetFemalePath(x, cards))
        // }).ToList();

        // a.cardEffect = names.Where(x => Animation.Instance.map.ContainsKey(x)).Select(x => new KeyValue<Spine.Unity.SkeletonDataAsset>
        // {
        //     key = x,
        //     value = Resources.Load<Spine.Unity.SkeletonDataAsset>($"cards/{Animation.Instance.map[x]}")
        // }).ToList();

        // a.cardSuit = KeyValueList.New(Sprites.Instance.cardSuit);
        // a.cardBlackWeight = Sprites.Instance.blackWeight.Values.ToArray();
        // a.cardRedWeight = Sprites.Instance.redWeight.Values.ToArray();
        // a.redShaEffect = Resources.Load<Spine.Unity.SkeletonDataAsset>($"cards/{Animation.Instance.map["红杀"]}");

        // a.selfEquipImage = KeyValueList.New(Sprites.Instance.equipImage);
        // a.selfEquipImage.Sort((x, y) => order(x.key).CompareTo(order(y.key)));

        // a.equipImage = KeyValueList.New(Sprites.Instance.seat_equip);
        // a.equipImage.Sort((x, y) => order(x.key).CompareTo(order(y.key)));

        // a.equipSuit = KeyValueList.New(Sprites.Instance.seat_suit);
        // a.equipBlackWeight = Sprites.Instance.seat_blackWeight.Values.ToArray();
        // a.equipRedWeight = Sprites.Instance.seat_redWeight.Values.ToArray();
        // a.judgeCardImage = KeyValueList.New(Sprites.Instance.judgeCard);

        // UnityEditor.AssetDatabase.CreateAsset(a, "Assets/GameAssets.asset");
        // UnityEditor.AssetDatabase.SaveAssets();
        // UnityEditor.AssetDatabase.Refresh();

        await GameCore.Game.Instance.Init();
        GameCore.Game.Instance.Run();
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<CardPanelQuery>(ShowPanel);
        EventSystem.Instance.RemoveEvent<FinishCardPanel>(HidePanel);

        EventSystem.Instance.RemoveEvent<InitPlayer>(Init);
        EventSystem.Instance.RemoveEvent<StartBanPick>(OnStartBP);
        EventSystem.Instance.RemoveEvent<FinishBanPick>(OnFinishBP);
        EventSystem.Instance.RemoveEvent<Model.GameOver>(OnGameOver);

        EventSystem.Instance.RemoveEvent<Die>(OnPlayerDead);

        EventSystem.Instance.RemovePriorEvent<StartTurn>(ChangeView);
        EventSystem.Instance.RemovePriorEvent<PlayQuery>(ChangeView);
    }

    private void SetBorder()
    {
        float x = GetComponent<RectTransform>().sizeDelta.x;
        float y = GetComponent<RectTransform>().sizeDelta.y;
        Debug.Log("canvas.x = " + x);
        float d = x / y > 2 ? x * 0.5f - y : 0;
        border.offsetMin = new Vector2(d, 0);
        border.offsetMax = new Vector2(-d, 0);
    }

    private void Init(InitPlayer startGame)
    {
        foreach (var i in GameModel.Instance.players)
        {
            var player = Instantiate(playerPrefab, i.isSelf ? _selfs : _enemys).GetComponent<Player>();
            player.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            player.Init(i);
            players.Add(player);
        }

        firstPerson.Init(players.Find(x => x.model.isSelf).model);
    }

    private void OnStartBP(StartBanPick startBanPick)
    {
        BanPick.Instance.Show(startBanPick.generals);
    }

    private void OnFinishBP(FinishBanPick finishBanPick)
    {
        Destroy(BanPick.Instance.gameObject);
        border.gameObject.SetActive(true);
    }

    private void OnGameOver(Model.GameOver gameOver)
    {
        GameOver.Instance.Show(gameOver.loser == selfTeam);
    }

    private void OnPlayerDead(Die model)
    {
        if (model.player == firstPerson.model.index)
        {
            var player = players.FirstOrDefault(x => x.model != firstPerson.model && x.model.isSelf);
            if (player != null) ChangeView(player.model);
        }
    }

    private void ChangeView(StartTurn startTurn) => ChangeView(Player.Find(startTurn.player).model);
    private void ChangeView(PlayQuery playQuery)
    {
        if (playQuery.origin.type != SinglePlayQuery.Type.WXKJ) ChangeView(Player.Find(playQuery.player).model);
    }

    private void ChangeView(Model.Player player)
    {
        if (player.isSelf && player != firstPerson.model)
        {
            firstPerson.Init(player);
            firstPerson.general.Init(player);
            OnChangeView?.Invoke(player);
        }
    }

    /// <summary>
    /// 更新座位
    /// </summary>
    public System.Action<Model.Player> OnChangeView;

    private void ShowPanel(CardPanelQuery cpr)
    {
        var player = Player.Find(cpr.player).model;
        if (firstPerson.model != player) return;
        ChangeView(player);
        CardPanel.Instance.Show(cpr);
    }

    private void HidePanel(FinishCardPanel fcp)
    {
        if (firstPerson.model.index != fcp.player) return;
        CardPanel.Instance.gameObject.SetActive(false);
    }

    // private List<string> bgUrl = new List<string>
    // {
    //     "10",
    //     "autoChessbeijing_s",
    //     "boyunjianri_s",
    //     "chengneidenghuo_s",
    //     "qunxiongbeijing_s",
    //     "shuguobeijing_s",
    //     "weiguobeijing_s",
    //     "wuguobeijing_s",
    //     "zhanchangbeijing_s"
    // };

    private int bgIndex;

    public void ChangeBg()
    {
        // string url = $"{Url.IMAGE}Background/{bgUrl[bgIndex++ % bgUrl.Count]}.jpeg";
        // background.texture = await WebRequest.GetTexture(url);

        background.sprite = gameAssets.background[bgIndex++ % gameAssets.background.Count];
        // 调整原始图像大小
        background.SetNativeSize();
        // 适应屏幕
        var rect = background.sprite.rect;
        Vector2 canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
        float radio = Mathf.Max(canvasSize.x / rect.width, canvasSize.y / rect.height);
        background.rectTransform.sizeDelta *= radio;
    }
}
