using UnityEngine;
using UnityEngine.UI;

public class Self : SingletonMono<Self>
{
    // 阶段信息
    public Image currentPhase;
    // 每阶段对应sprite
    public Sprite[] phaseSprite;

    public Button teammate;
    public GameObject teammatePanel;

    private GameCore.Player model => GameMain.Instance.self.model;

    private void Start()
    {
        currentPhase.gameObject.SetActive(false);
        teammate.onClick.AddListener(ClickTeammate);

        GameCore.TurnSystem.Instance.StartPhaseView += ShowPhase;
        GameCore.TurnSystem.Instance.FinishPhaseView += HidePhase;
    }

    private void OnDestroy()
    {
        GameCore.TurnSystem.Instance.StartPhaseView -= ShowPhase;
        GameCore.TurnSystem.Instance.FinishPhaseView -= HidePhase;
    }

    /// <summary>
    /// 显示并更新阶段信息
    /// </summary>
    public void ShowPhase()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;

        currentPhase.gameObject.SetActive(true);
        currentPhase.sprite = phaseSprite[(int)GameCore.TurnSystem.Instance.CurrentPhase];
    }

    /// <summary>
    /// 隐藏阶段信息(回合外)
    /// </summary>
    public void HidePhase()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;
        currentPhase.gameObject.SetActive(false);
    }

    private void ClickTeammate()
    {
        teammatePanel.SetActive(true);
    }
}