using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<string, ElsePlayerEquip> equipments;

    private Player player;
    private Model.Player model => player.model;

    private void Awake()
    {
        // 阶段信息
        EventSystem.Instance.AddEvent<StartPhase>(ShowPhase);
        EventSystem.Instance.AddEvent<FinishPhase>(HidePhase);

        // 出牌
        EventSystem.Instance.AddEvent<PlayQuery>(ShowTimer);
        EventSystem.Instance.AddEvent<FinishPlay>(HideTimer);
        EventSystem.Instance.AddEvent<CardPanelQuery>(ShowTimer);
        EventSystem.Instance.AddEvent<FinishCardPanel>(HideTimer);

        // 手牌
        EventSystem.Instance.AddEvent<UpdateCard>(UpdateHandCardCount);
        EventSystem.Instance.AddEvent<LoseCard>(OnLoseCard);

        // 装备区
        EventSystem.Instance.AddEvent<AddEquipment>(OnAddEquipment);

        player = GetComponent<Player>();

        currentPhase.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);

        equipments = new Dictionary<string, ElsePlayerEquip>
        {
            {"武器", equipArray[0]},
            {"防具", equipArray[1]},
            {"加一马", equipArray[2]},
            {"减一马", equipArray[3]}
        };

        if (model.isSelf) handCardCount.transform.parent.gameObject.AddComponent<HandCardPointerHandler>();
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<StartPhase>(ShowPhase);
        EventSystem.Instance.RemoveEvent<FinishPhase>(HidePhase);

        EventSystem.Instance.RemoveEvent<PlayQuery>(ShowTimer);
        EventSystem.Instance.RemoveEvent<FinishPlay>(HideTimer);
        EventSystem.Instance.RemoveEvent<CardPanelQuery>(ShowTimer);
        EventSystem.Instance.RemoveEvent<FinishCardPanel>(HideTimer);

        EventSystem.Instance.RemoveEvent<UpdateCard>(UpdateHandCardCount);

        EventSystem.Instance.RemoveEvent<AddEquipment>(OnAddEquipment);
        EventSystem.Instance.RemoveEvent<LoseCard>(OnLoseCard);
    }

    private bool IsSelf(int player, SinglePlayQuery.Type type)
    {
        return player == model.index || type == SinglePlayQuery.Type.WXKJ && Player.Find(player).model.team == model.team;
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

    private void ShowTimer(PlayQuery playQuery)
    {
        if (IsSelf(playQuery.player, playQuery.origin.type)) ShowTimer(playQuery.second);
    }

    private void ShowTimer(CardPanelQuery cpq)
    {
        if (!gameObject.activeSelf || cpq.player != model.index) return;
        ShowTimer(cpq.second);
    }

    /// <summary>
    /// 隐藏倒计时进度条
    /// </summary>
    private void HideTimer(FinishPlay finishPlay)
    {
        if (IsSelf(finishPlay.player, finishPlay.type))
        {
            StopAllCoroutines();
            slider.gameObject.SetActive(false);
        }
    }

    private void HideTimer(FinishCardPanel fcp)
    {
        if (fcp.player == model.index)
        {
            StopAllCoroutines();
            slider.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示并更新阶段信息
    /// </summary>
    private void ShowPhase(StartPhase startPhase)
    {
        if (startPhase.player != model.index) return;

        currentPhase.gameObject.SetActive(true);
        currentPhase.sprite = phaseSprite[(int)startPhase.phase];
    }

    /// <summary>
    /// 隐藏阶段信息(回合外)
    /// </summary>
    private void HidePhase(FinishPhase finishPhase)
    {
        if (finishPhase.player != model.index) return;
        currentPhase.gameObject.SetActive(false);
    }

    private void UpdateHandCardCount(UpdateCard updateCard)
    {
        if (updateCard.player != model.index) return;
        handCardCount.text = model.handCards.Count.ToString();
    }

    private void OnAddEquipment(AddEquipment addEquipment)
    {
        if (addEquipment.player != model.index) return;
        int id = addEquipment.card;
        equipments[Model.Card.Find(id).type].Show(id);
    }

    private void OnLoseCard(LoseCard loseCard)
    {
        if (loseCard.player != model.index) return;
        foreach (var i in equipments.Values.Where(x => loseCard.cards.Contains(x.id))) i.Hide();
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

class HandCardPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GeneralInfo.Instance.gameObject.activeSelf) TeammateHandCardPanel.Instance.Show(player.model);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TeammateHandCardPanel.Instance.gameObject.SetActive(false);
    }
}
