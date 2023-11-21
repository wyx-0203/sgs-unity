using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameMain : SingletonMono<GameMain>
{
    public RawImage background;

    public List<Player> players { get; } = new();
    public Player self;
    public GameObject gameOver;
    public RectTransform border;
    public GameObject bp;

    public GameObject cardPanel;

    public GameObject playerPrefab;
    public Transform _selfs;
    public Transform _enemys;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        ABManager.Instance.LoadGameScene().Wait();

#elif UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif
    }

    private async void Start()
    {

        SetBorder();
        bgIndex = Random.Range(0, bgUrl.Count);
        ChangeBg();
        BGM.Instance.Load(Url.AUDIO + "bgm/bgm_1.mp3");

        GameCore.CardPanel.Instance.StartTimerView += ShowPanel;
        GameCore.CardPanel.Instance.StopTimerView += HidePanel;

        GameCore.BanPick.Instance.ShowPanelView += ShowBP;
        GameCore.Main.Instance.AfterBanPickView += Init;

        GameCore.Main.Instance.MoveSeatView += MoveSeat;

        GameCore.Main.Instance.GameOverView += GameOver;

        await GameCore.Main.Instance.Init();
        GameCore.Main.Instance.Run();
    }

    private void OnDestroy()
    {
        GameCore.CardPanel.Instance.StartTimerView -= ShowPanel;
        GameCore.CardPanel.Instance.StopTimerView -= HidePanel;

        GameCore.BanPick.Instance.ShowPanelView -= ShowBP;
        GameCore.Main.Instance.AfterBanPickView -= Init;

        GameCore.Main.Instance.MoveSeatView -= MoveSeat;

        GameCore.Main.Instance.GameOverView -= GameOver;
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

    private void ShowBP()
    {
        bp.SetActive(true);
    }

    /// <summary>
    /// 初始化每个View.Player
    /// </summary>
    private void Init()
    {
        Destroy(bp);
        border.gameObject.SetActive(true);

        foreach (var i in GameCore.Main.Instance.players)
        {
            var player = Instantiate(playerPrefab).GetComponent<Player>();
            player.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            player.Init(i);
            players.Add(player);
            player.transform.SetParent(i.isSelf ? _selfs : _enemys, false);
        }

        self.Init(GameCore.Main.Instance.AlivePlayers.Find(x => x.isSelf));
    }

    private void GameOver()
    {
        gameOver.SetActive(true);
    }

    private void OnPlayerDie(GameCore.Die model)
    {
        if (model.player == self.model)
        {
            var player = self.model.team.GetAllPlayers().Where(x => x.alive).FirstOrDefault();
            if (player != null) MoveSeat(player);
        }
    }

    /// <summary>
    /// 更新座位
    /// </summary>
    private void MoveSeat(GameCore.Player model)
    {
        self.Init(model);
    }

    private void ShowPanel(GameCore.CardPanel model)
    {
        if (self.model != model.player) return;
        cardPanel.SetActive(true);
    }

    private void HidePanel(GameCore.CardPanel model)
    {
        if (self.model != model.player) return;
        cardPanel.SetActive(false);
    }

    private List<string> bgUrl = new List<string>
        {
            "10",
            "autoChessbeijing_s",
            "boyunjianri_s",
            "chengneidenghuo_s",
            "qunxiongbeijing_s",
            "shuguobeijing_s",
            "weiguobeijing_s",
            "wuguobeijing_s",
            "zhanchangbeijing_s"
        };

    private int bgIndex;

    public async void ChangeBg()
    {
        string url = Url.IMAGE + "Background/" + bgUrl[bgIndex++ % bgUrl.Count] + ".jpeg";
        background.texture = await WebRequest.GetTexture(url);

        // 调整原始图像大小，以使其像素精准。
        background.SetNativeSize();
        // 适应屏幕
        Texture texture = background.texture;
        Vector2 canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
        float radio = Mathf.Max(canvasSize.x / texture.width, canvasSize.y / texture.height);
        background.rectTransform.sizeDelta *= radio;
    }
}