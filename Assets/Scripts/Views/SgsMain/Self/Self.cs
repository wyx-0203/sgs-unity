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

        public Button teammate;
        public GameObject teammatePanel;

        private Model.Player model => SgsMain.Instance.self.model;

        private void Start()
        {
            currentPhase.gameObject.SetActive(false);
            teammate.onClick.AddListener(ClickTeammate);

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
        public void ShowPhase()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(true);
            currentPhase.sprite = phaseSprite[(int)Model.TurnSystem.Instance.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        public void HidePhase()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;
            currentPhase.gameObject.SetActive(false);
        }

        private void ClickTeammate()
        {
            teammatePanel.SetActive(true);
        }
    }
}
