using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElsePlayer : MonoBehaviour
{
    public Image currentPhase;
    public Sprite[] phaseSprite;
    public Slider slider;
    public Text handCardCount;

    public ElsePlayerEquip[] equipArray;
    private Dictionary<string, ElsePlayerEquip> equipages;

    private Player player;
    private GameCore.Player model => player.model;
    private GameCore.Timer timer => GameCore.Timer.Instance;

    private void Start()
    {
        // 阶段信息
        GameCore.TurnSystem.Instance.StartPhaseView += ShowPhase;
        GameCore.TurnSystem.Instance.FinishPhaseView += HidePhase;

        // 进度条
        GameCore.Timer.Instance.StartTimerView += ShowTimer;
        GameCore.Timer.Instance.StopTimerView += HideTimer;
        GameCore.CardPanel.Instance.StartTimerView += ShowTimer;
        GameCore.CardPanel.Instance.StopTimerView += HideTimer;

        // 获得牌
        GameCore.GetCard.ActionView += UpdateHandCardCount;

        // 失去牌
        GameCore.LoseCard.ActionView += UpdateHandCardCount;

        // 装备区
        GameCore.Equipment.AddEquipView += ShowEquip;
        GameCore.Equipment.RemoveEquipView += HideEquip;

        player = GetComponent<Player>();

        currentPhase.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);

        equipages = new Dictionary<string, ElsePlayerEquip>
            {
                {"武器", equipArray[0]},
                {"防具", equipArray[1]},
                {"加一马", equipArray[2]},
                {"减一马", equipArray[3]}
            };

        UpdateHandCardCount();

        if (model.isSelf) handCardCount.transform.parent.gameObject.AddComponent<HandCardPointerHandler>();
        // phaseSprite = Sprites.Instance.phase;
    }

    private void OnDestroy()
    {
        GameCore.TurnSystem.Instance.StartPhaseView -= ShowPhase;
        GameCore.TurnSystem.Instance.FinishPhaseView -= HidePhase;

        GameCore.Timer.Instance.StartTimerView -= ShowTimer;
        GameCore.Timer.Instance.StopTimerView -= HideTimer;
        GameCore.CardPanel.Instance.StartTimerView -= ShowTimer;
        GameCore.CardPanel.Instance.StopTimerView -= HideTimer;

        GameCore.GetCard.ActionView -= UpdateHandCardCount;

        GameCore.LoseCard.ActionView -= UpdateHandCardCount;

        GameCore.Equipment.AddEquipView -= ShowEquip;
        GameCore.Equipment.RemoveEquipView -= HideEquip;
    }

    /// <summary>
    /// 显示倒计时进度条
    /// </summary>
    private void ShowTimer(int second)
    {
        slider.gameObject.SetActive(true);
        slider.value = 1;
        StartCoroutine(UpdateTimer(second));
    }

    private void ShowTimer()
    {
        if (!gameObject.activeSelf || !timer.players.Contains(model)) return;
        ShowTimer(timer.second);
    }

    private void ShowTimer(GameCore.CardPanel cardPanel)
    {
        if (!gameObject.activeSelf || cardPanel.player != model) return;
        ShowTimer(cardPanel.second);
    }

    /// <summary>
    /// 隐藏倒计时进度条
    /// </summary>
    private void HideTimer()
    {
        if (!gameObject.activeSelf || !timer.players.Contains(model)) return;
        StopAllCoroutines();
        slider.gameObject.SetActive(false);
    }

    private void HideTimer(GameCore.CardPanel cardPanel)
    {
        if (cardPanel.player != model) return;
        HideTimer();
    }

    /// <summary>
    /// 显示并更新阶段信息
    /// </summary>
    private void ShowPhase()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;

        currentPhase.gameObject.SetActive(true);

        currentPhase.sprite = phaseSprite[(int)GameCore.TurnSystem.Instance.CurrentPhase];
    }

    /// <summary>
    /// 隐藏阶段信息(回合外)
    /// </summary>
    private void HidePhase()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;

        currentPhase.gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新手牌数
    /// </summary>
    private void UpdateHandCardCount()
    {
        handCardCount.text = model.handCardsCount.ToString();
    }

    private void UpdateHandCardCount(GameCore.GetCard operation)
    {
        if (operation.player != model) return;
        UpdateHandCardCount();
    }

    private void UpdateHandCardCount(GameCore.LoseCard operation)
    {
        if (operation.player != model) return;
        UpdateHandCardCount();
    }

    private void ShowEquip(GameCore.Equipment card)
    {
        if (card.owner != model) return;

        equipages[card.type].gameObject.SetActive(true);
        equipages[card.type].Init(card);
    }

    private void HideEquip(GameCore.Equipment card)
    {
        if (card.owner != model) return;
        if (card.id != equipages[card.type].Id) return;

        equipages[card.type].gameObject.SetActive(false);
    }

    /// <summary>
    /// 每帧更新进度条
    /// </summary>
    private IEnumerator UpdateTimer(int second)
    {
        while (slider.value > 0)
        {
            slider.value -= Time.deltaTime / second;
            yield return null;
        }
    }
}

class HandCardPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Player player;
    private TeammateHandCardPanel teammateHandCardPanel;

    private void Start()
    {
        player = GetComponentInParent<Player>();
        teammateHandCardPanel = GameMain.Instance.transform.Find("队友手牌Panel").GetComponent<TeammateHandCardPanel>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        teammateHandCardPanel.Show(player.model);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        teammateHandCardPanel.Show(player.model);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        teammateHandCardPanel.gameObject.SetActive(false);
    }
}
