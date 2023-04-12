using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Self : SingletonMono<Self>
    {
        // 阶段信息
        public Image currentPhase;
        // 每阶段对应sprite
        public Sprite[] phaseSprite;

        public Button changeSkin;
        public Button changeBg;
        public Button teammate;
        public Button surrender;
        public GameObject teammatePanel;

        private Model.Player model => SgsMain.Instance.self.model;

        private void Start()
        {
            currentPhase.gameObject.SetActive(false);
            changeSkin.onClick.AddListener(ChangeSkin);
            changeBg.onClick.AddListener(ChangeBg);
            teammate.onClick.AddListener(ClickTeammate);
            surrender.onClick.AddListener(ClickSurrender);

            Model.TurnSystem.Instance.StartPhaseView += ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView += HidePhase;
        }

        private void OnDestroy()
        {
            Model.TurnSystem.Instance.StartPhaseView -= ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView -= HidePhase;
        }

        /// <summary>
        /// 显示并更新阶段信息
        /// </summary>
        public void ShowPhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(true);
            currentPhase.sprite = phaseSprite[(int)turnSystem.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        public void HidePhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;
            currentPhase.gameObject.SetActive(false);
        }

        private void ChangeSkin()
        {
            model.SendChangeSkin();
        }

        private void ChangeBg()
        {
            SgsMain.Instance.ChangeBg();
        }

        private void ClickTeammate()
        {
            teammatePanel.SetActive(true);
        }

        private void ClickSurrender()
        {
            Model.GameOver.Instance.SendSurrender();
            if (Model.Timer.Instance.isPerformPhase) Model.Timer.Instance.SendResult();
        }
    }
}
