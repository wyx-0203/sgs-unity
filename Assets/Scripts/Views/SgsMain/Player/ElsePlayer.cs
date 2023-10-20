using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View
{
    public class ElsePlayer : MonoBehaviour
    {
        public Image currentPhase;
        public Sprite[] phaseSprite;
        public Slider slider;
        public Text handCardCount;

        public ElsePlayerEquip[] equipArray;
        private Dictionary<string, ElsePlayerEquip> equipages;

        private Player player;
        private Model.Player model => player.model;
        private Model.Timer timer => Model.Timer.Instance;

        private void Start()
        {
            // 阶段信息
            Model.TurnSystem.Instance.StartPhaseView += ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView += HidePhase;

            // 进度条
            Model.Timer.Instance.StartTimerView += ShowTimer;
            Model.Timer.Instance.StopTimerView += HideTimer;
            Model.CardPanel.Instance.StartTimerView += ShowTimer;
            Model.CardPanel.Instance.StopTimerView += HideTimer;

            // 获得牌
            Model.GetCard.ActionView += UpdateHandCardCount;

            // 失去牌
            Model.LoseCard.ActionView += UpdateHandCardCount;

            // 装备区
            Model.Equipment.AddEquipView += ShowEquip;
            Model.Equipment.RemoveEquipView += HideEquip;

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
            Model.TurnSystem.Instance.StartPhaseView -= ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView -= HidePhase;

            Model.Timer.Instance.StartTimerView -= ShowTimer;
            Model.Timer.Instance.StopTimerView -= HideTimer;
            Model.CardPanel.Instance.StartTimerView -= ShowTimer;
            Model.CardPanel.Instance.StopTimerView -= HideTimer;

            Model.GetCard.ActionView -= UpdateHandCardCount;

            Model.LoseCard.ActionView -= UpdateHandCardCount;

            Model.Equipment.AddEquipView -= ShowEquip;
            Model.Equipment.RemoveEquipView -= HideEquip;
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

        private void ShowTimer(Model.CardPanel cardPanel)
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

        private void HideTimer(Model.CardPanel cardPanel)
        {
            if (cardPanel.player != model) return;
            HideTimer();
        }

        /// <summary>
        /// 显示并更新阶段信息
        /// </summary>
        private void ShowPhase()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(true);

            currentPhase.sprite = phaseSprite[(int)Model.TurnSystem.Instance.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        private void HidePhase()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新手牌数
        /// </summary>
        private void UpdateHandCardCount()
        {
            handCardCount.text = model.HandCardCount.ToString();
        }

        private void UpdateHandCardCount(Model.GetCard operation)
        {
            if (operation.player != model) return;
            UpdateHandCardCount();
        }

        private void UpdateHandCardCount(Model.LoseCard operation)
        {
            if (operation.player != model) return;
            UpdateHandCardCount();
        }

        private void ShowEquip(Model.Equipment card)
        {
            if (card.Owner != model) return;

            equipages[card.type].gameObject.SetActive(true);
            equipages[card.type].Init(card);
        }

        private void HideEquip(Model.Equipment card)
        {
            if (card.Owner != model) return;
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
            teammateHandCardPanel = SgsMain.Instance.transform.Find("队友手牌Panel").GetComponent<TeammateHandCardPanel>();
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

}