using Model;
using UnityEngine;
using UnityEngine.UI;

public class Self : SingletonMono<Self>
{
    // 阶段信息
    public Image currentPhase;
    // 每阶段对应sprite
    public Sprite[] phaseSprite;

    // public Button teammate;
    public GameObject teammatePanel;

    private int self => Game.Instance.firstPerson.model.index;

    private void Start()
    {
        currentPhase.gameObject.SetActive(false);
        // teammate.onClick.AddListener(ClickTeammate);

        EventSystem.Instance.AddEvent<StartPhase>(ShowPhase);
        EventSystem.Instance.AddEvent<FinishPhase>(HidePhase);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<StartPhase>(ShowPhase);
        EventSystem.Instance.RemoveEvent<FinishPhase>(HidePhase);
    }

    /// <summary>
    /// 显示并更新阶段信息
    /// </summary>
    public void ShowPhase(StartPhase startPhase)
    {
        if (startPhase.player != self) return;

        currentPhase.gameObject.SetActive(true);
        currentPhase.sprite = phaseSprite[(int)startPhase.phase];
    }

    /// <summary>
    /// 隐藏阶段信息(回合外)
    /// </summary>
    public void HidePhase(FinishPhase finishPhase)
    {
        if (finishPhase.player != self) return;
        currentPhase.gameObject.SetActive(false);
    }

    // private void ClickTeammate()
    // {
    //     teammatePanel.SetActive(true);
    // }
}