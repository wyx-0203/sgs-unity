using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayArea : SingletonMono<PlayArea>
{
    // 操作区
    public GameObject operationArea;
    // 倒计时读条
    public Slider slider;
    // 提示
    public Text hint;
    // 按键栏
    public GameObject buttonBar;
    // 确定键
    public Button confirm;
    // 取消键
    public Button cancel;
    // 回合结束键
    public Button finishPhase;

    public SinglePlayQuery current { get; set; }
    public PlayQuery playQuery { get; private set; }
    // public List<SinglePlayQuery> skills{get;private set;}
    public PlayDecision decision { get; private set; }

    private void Start()
    {
        confirm.onClick.AddListener(ClickConfirm);
        cancel.onClick.AddListener(ClickCancel);
        finishPhase.onClick.AddListener(ClickFinishPhase);

        // confirm.onClick.AddListener(HideTimer);
        // cancel.onClick.AddListener(HideTimer);
        // finishPhase.onClick.AddListener(HideTimer);

        HideTimer();

        EventSystem.Instance.AddEvent<PlayQuery>(OnStartPlay);
        EventSystem.Instance.AddEvent<FinishPlay>(HideTimer);
        EventSystem.Instance.AddEvent<Model.GameOver>(OnGameOver);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<PlayQuery>(OnStartPlay);
        EventSystem.Instance.RemoveEvent<FinishPlay>(HideTimer);
        EventSystem.Instance.RemoveEvent<Model.GameOver>(OnGameOver);
    }

    /// <summary>
    /// 点击确定键
    /// </summary>
    private void ClickConfirm()
    {
        // playRequest.temp.action = true;
        // if (playRequest.type == GameCore.Timer.Type.WXKJ) playRequest.temp.src = playRequest.temp.cards.FirstOrDefault()?.src;
        // playRequest.SendDecision();

        decision.action = true;
        EventSystem.Instance.Send(decision);
        HideTimer();
    }

    /// <summary>
    /// 点击取消键
    /// </summary>
    private void ClickCancel()
    {
        // 取消技能
        if (current != playQuery.origin)
        {
            SkillArea.Instance.UnSelect();
            return;
        }
        decision.Clear();
        EventSystem.Instance.Send(decision);
        HideTimer();

        // var d = await Model.MCTS.Instance.Run();
        // // Debug.Log(Model.Decision.List.Instance == Model.MCTS.Instance._decisionList);
        // // Debug.Log(Model.Decision.List.Instance);
        // timer.SendDecision(d);
        // Debug.Log(Model.Decision.List.Instance);

        // if (Config.Instance.selfAI) playRequest.SendDecision(Config.Instance.EnableMCTS ? await GameCore.MCTS.Instance.Run(GameCore.MCTS.State.WaitTimer) : playRequest.DefaultAI());
        // else playRequest.SendDecision();
    }

    public bool DebugAI;

    /// <summary>
    /// 点击回合结束键
    /// </summary>
    private void ClickFinishPhase()
    {
        // StopAllCoroutines();
        decision.Clear();
        EventSystem.Instance.Send(decision);
        HideTimer();

        // if (Config.Instance.selfAI) playRequest.SendDecision(Config.Instance.EnableMCTS ? await GameCore.MCTS.Instance.Run(GameCore.MCTS.State.WaitTimer) : playRequest.DefaultAI());
        // if (Config.Instance.selfAI) timer.SendDecision(Model.MCTS.Instance.state == Model.MCTS.State.Disable ? timer.DefaultAI() : await Model.MCTS.Instance.Run());
        // if (Config.Instance.selfAI) timer.SendDecision(timer.DefaultAI());
        // else playRequest.SendDecision();
    }

    /// <summary>
    /// 开始出牌
    /// </summary>
    public void OnStartPlay(PlayQuery playQuery)
    {
        if (!Player.IsSelf(playQuery.player)) return;
        this.playQuery = playQuery;
        current = playQuery.origin;
        decision = new PlayDecision { player = playQuery.player, skill = current.skillName };

        operationArea.SetActive(true);
        hint.text = current.hint;

        // 初始化进度条和按键

        confirm.gameObject.SetActive(true);
        cancel.gameObject.SetActive(playQuery.refusable);
        finishPhase.gameObject.SetActive(current.type == SinglePlayQuery.Type.PlayPhase);

        // await Util.WaitFrame(2);
        SkillArea.Instance.OnStartPlay();
        CardArea.Instance.OnStartPlay();
        EquipArea.Instance.OnStartPlay();
        VirtualCardArea.Instance.OnStartPlay();
        DestArea.Instance.OnStartPlay();

        UpdateButtonArea();
        StartCoroutine(StartTimer(playQuery.second));
    }

    /// <summary>
    /// 隐藏进度条
    /// </summary>
    private void HideTimer()
    {
        // 隐藏所有按键
        StopAllCoroutines();
        confirm.gameObject.SetActive(false);
        cancel.gameObject.SetActive(false);
        finishPhase.gameObject.SetActive(false);
        operationArea.SetActive(false);
    }

    private void HideTimer(FinishPlay finishPlay)
    {
        if (Player.IsSelf(finishPlay.player)) HideTimer();
    }

    /// <summary>
    /// 开始倒计时
    /// </summary>
    private IEnumerator StartTimer(int second)
    {
        slider.value = 1;
        while (slider.value > 0)
        {
            slider.value -= Time.deltaTime / second;
            yield return null;
        }
    }

    /// <summary>
    /// 更新按键区
    /// </summary>
    public void UpdateButtonArea()
    {
        // 启用确定键 cardArea.IsValid &&
        confirm.interactable = DestArea.Instance.IsValid;
        // 出牌阶段，取消键用于取消选中技能
        cancel.interactable = current.type != SinglePlayQuery.Type.PlayPhase || current.skillName != "";
    }

    private void OnGameOver(Model.GameOver gameOver)
    {
        operationArea.SetActive(false);
    }
}