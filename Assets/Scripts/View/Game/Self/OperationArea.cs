using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class OperationArea : SingletonMono<OperationArea>
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

    private Player self => GameMain.Instance.self;
    private CardArea cardArea => CardArea.Instance;
    private DestArea destArea => DestArea.Instance;
    private EquipArea equipArea => EquipArea.Instance;
    private SkillArea skillArea => SkillArea.Instance;

    private GameCore.Decision model => timer.temp;
    private GameCore.Timer timer => GameCore.Timer.Instance;

    private void Start()
    {
        confirm.onClick.AddListener(ClickConfirm);
        cancel.onClick.AddListener(ClickCancel);
        finishPhase.onClick.AddListener(ClickFinishPhase);

        HideTimer();

        GameCore.Timer.Instance.StartTimerView += OnStartPlay;
        GameCore.Timer.Instance.StopTimerView += HideTimer;
        GameCore.Main.Instance.GameOverView += OnGameOver;
    }

    private void OnDestroy()
    {
        GameCore.Timer.Instance.StartTimerView -= OnStartPlay;
        GameCore.Timer.Instance.StopTimerView -= HideTimer;
        GameCore.Main.Instance.GameOverView -= OnGameOver;
    }

    /// <summary>
    /// 点击确定键
    /// </summary>
    private void ClickConfirm()
    {
        // StopAllCoroutines();

        timer.temp.action = true;
        if (timer.type == GameCore.Timer.Type.WXKJ) timer.temp.src = timer.temp.cards.FirstOrDefault()?.src;
        timer.SendDecision();
    }

    /// <summary>
    /// 点击取消键
    /// </summary>
    private async void ClickCancel()
    {
        // 取消技能
        if (model.skill != null && model.skill is not GameCore.Triggered)
        {
            skillArea.UnSelect();
            // skillArea.ClickSkill(model.skill);
            return;
        }

        // var d = await Model.MCTS.Instance.Run();
        // // Debug.Log(Model.Decision.List.Instance == Model.MCTS.Instance._decisionList);
        // // Debug.Log(Model.Decision.List.Instance);
        // timer.SendDecision(d);
        // Debug.Log(Model.Decision.List.Instance);

        if (Config.Instance.selfAI) timer.SendDecision(Config.Instance.EnableMCTS ? await GameCore.MCTS.Instance.Run(GameCore.MCTS.State.WaitTimer) : timer.DefaultAI());
        else timer.SendDecision();
    }

    public bool DebugAI;

    /// <summary>
    /// 点击回合结束键
    /// </summary>
    private async void ClickFinishPhase()
    {
        // StopAllCoroutines();

        if (Config.Instance.selfAI) timer.SendDecision(Config.Instance.EnableMCTS ? await GameCore.MCTS.Instance.Run(GameCore.MCTS.State.WaitTimer) : timer.DefaultAI());
        // if (Config.Instance.selfAI) timer.SendDecision(Model.MCTS.Instance.state == Model.MCTS.State.Disable ? timer.DefaultAI() : await Model.MCTS.Instance.Run());
        // if (Config.Instance.selfAI) timer.SendDecision(timer.DefaultAI());
        else timer.SendDecision();
    }

    /// <summary>
    /// 显示倒计时进度条
    /// </summary>
    public async void OnStartPlay()
    {
        if (!timer.players.Contains(self.model)) return;
        await Util.WaitFrame(2);

        operationArea.SetActive(true);
        hint.text = timer.hint;

        // 初始化进度条和按键

        confirm.gameObject.SetActive(true);
        cancel.gameObject.SetActive(timer.refusable);
        finishPhase.gameObject.SetActive(timer.type == GameCore.Timer.Type.InPlayPhase);

        skillArea.OnStartPlay();
        cardArea.OnStartPlay();
        // cardArea.InitConvertCard();
        destArea.OnStartPlay();
        equipArea.OnStartPlay();

        UpdateButtonArea();
        StartCoroutine(StartTimer(timer.second));
    }

    /// <summary>
    /// 隐藏进度条
    /// </summary>
    public void HideTimer()
    {
        if (!timer.players.Contains(self.model)) return;

        // 隐藏所有按键
        StopAllCoroutines();
        confirm.gameObject.SetActive(false);
        cancel.gameObject.SetActive(false);
        finishPhase.gameObject.SetActive(false);
        operationArea.SetActive(false);
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
        // 启用确定键
        confirm.interactable = cardArea.IsValid && destArea.IsValid;
        // 出牌阶段，取消键用于取消选中技能
        cancel.interactable = timer.type != GameCore.Timer.Type.InPlayPhase || model.skill != null;
    }

    public void OnClickSkill()
    {
        // if(timer.isDone) return;

        cardArea.Reset();
        destArea.Reset();
        equipArea.Reset();

        skillArea.OnStartPlay();
        cardArea.OnStartPlay();
        // cardArea.InitConvertCard();
        destArea.OnStartPlay();
        equipArea.OnStartPlay();

        UpdateButtonArea();
    }

    private void OnGameOver()
    {
        operationArea.SetActive(false);
    }
}