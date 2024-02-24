using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class Game : SingletonMono<Game>
{
    public GameAsset gameAssets;

    public Image background;

    public List<Player> players;

    /// <summary>
    /// 主视角玩家
    /// </summary>
    public Player firstPerson;
    public RectTransform border;

    private async void Start()
    {
#if UNITY_EDITOR
        await ABManager.Instance.LoadGameScene();
#endif

        SetBorder();
        bgIndex = Random.Range(0, gameAssets.background.Count);
        ChangeBg();
        BGM.Instance.Load(gameAssets.bgm);

        EventSystem.Instance.AddEvent<CardPanelQuery>(ShowPanel);
        EventSystem.Instance.AddEvent<FinishCardPanel>(HidePanel);

        EventSystem.Instance.AddEvent<InitPlayer>(Init);
        EventSystem.Instance.AddEvent<StartBanPick>(OnStartBP);
        EventSystem.Instance.AddPriorEvent<FinishBanPick>(OnFinishBP);
        EventSystem.Instance.AddEvent<Model.GameOver>(OnGameOver);

        EventSystem.Instance.AddEvent<Die>(OnPlayerDead);

        EventSystem.Instance.AddPriorEvent<StartTurn>(ChangeView);
        EventSystem.Instance.AddPriorEvent<PlayQuery>(ChangeView);

        EventSystem.Instance.UnLock();

        if (Global.Instance.IsStandalone)
        {
            game = new GameCore.Game
            (
                Mode._3V3,
                new List<int> { User.StandaloneId, User.AIId },
                EventSystem.Instance.OnMessage,
                Team.RED
            );
            try { await game.Init(); }
            catch (System.Exception) { return; }
            await game.Run();
        }
    }

    public GameCore.Game game;

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<CardPanelQuery>(ShowPanel);
        EventSystem.Instance.RemoveEvent<FinishCardPanel>(HidePanel);

        EventSystem.Instance.RemoveEvent<InitPlayer>(Init);
        EventSystem.Instance.RemoveEvent<StartBanPick>(OnStartBP);
        EventSystem.Instance.RemovePriorEvent<FinishBanPick>(OnFinishBP);
        EventSystem.Instance.RemoveEvent<Model.GameOver>(OnGameOver);

        EventSystem.Instance.RemoveEvent<Die>(OnPlayerDead);

        EventSystem.Instance.RemovePriorEvent<StartTurn>(ChangeView);
        EventSystem.Instance.RemovePriorEvent<PlayQuery>(ChangeView);

        if (Global.Instance.IsStandalone)
        {
            EventSystem.Instance.SendToServer(new Surrender { team = Team.None });
        }
    }

    private void SetBorder()
    {
        float x = GetComponent<RectTransform>().sizeDelta.x;
        float y = GetComponent<RectTransform>().sizeDelta.y;
        Debug.Log("canvas.x = " + x);
        float t = 0.2f;
        float d = (x - y * 16f / 9f) * t;
        if (d > 0)
        {
            border.offsetMin = new Vector2(d, 0);
            border.offsetMax = new Vector2(-d, 0);
        }
    }

    private void Init(InitPlayer initPlayer)
    {
        if (GameModel.Instance.selfTeam == Team.RED)
        {
            for (int i = 0; i < 3; i++)
            {
                (players[i + 3], players[i]) = (players[i], players[i + 3]);
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].Init(GameModel.Instance.players[i]);
        }

        firstPerson.Init(GameModel.Instance.players.First(x => x.isSelf));
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
        GameOver.Instance.Show(gameOver.loser == GameModel.Instance.selfTeam);
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
        if (!player.isSelf) return;
        ChangeView(player);
        CardPanel.Instance.Show(cpr);
    }

    private void HidePanel(FinishCardPanel fcp)
    {
        if (firstPerson.model.index != fcp.player) return;
        CardPanel.Instance.gameObject.SetActive(false);
    }

    private int bgIndex;

    public void ChangeBg()
    {
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
