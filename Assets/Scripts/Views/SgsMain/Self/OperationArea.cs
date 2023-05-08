using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
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

        private Player self => SgsMain.Instance.self;
        private CardArea cardArea => CardArea.Instance;
        private DestArea destArea => DestArea.Instance;
        private EquipArea equipArea => EquipArea.Instance;
        private SkillArea skillArea => SkillArea.Instance;

        private Model.Operation model => Model.Operation.Instance;
        private Model.Timer timerTask => Model.Timer.Instance;
        private Model.Timer timer;

        private void Start()
        {
            confirm.onClick.AddListener(ClickConfirm);
            cancel.onClick.AddListener(ClickCancel);
            finishPhase.onClick.AddListener(ClickFinishPhase);

            HideTimer();

            Model.Timer.Instance.StartTimerView += ShowTimer;
            Model.Timer.Instance.StopTimerView += HideTimer;
        }

        private void OnDestroy()
        {
            Model.Timer.Instance.StartTimerView -= ShowTimer;
            Model.Timer.Instance.StopTimerView -= HideTimer;
        }

        // private Model.Player player => self.model;
        // private List<Model.Card> cards => Model.CardPile.Instance.cards;

        /// <summary>
        /// 点击确定键
        /// </summary>
        private void ClickConfirm()
        {
            StopAllCoroutines();

            List<int> cards = new List<int>();
            foreach (var i in model.Cards) cards.Add(i.Id);
            foreach (var i in model.Equips) cards.Add(i.Id);

            List<int> players = new List<int>();
            foreach (var i in model.Dests) players.Add(i.position);

            string skillName = model.skill != null ? model.skill.Name : "";

            if (timerTask.isWxkj)
            {
                bool isSelf = self.model.HandCards.Contains(Model.CardPile.Instance.cards[cards[0]]);
                timerTask.SendResult((isSelf ? self.model : self.model.teammate).position, true, cards);
            }
            else if (timerTask.isCompete)
            {
                HideTimer();
                timerTask.SendResult(self.model.position, true, cards);
            }
            else
            {
                string other = model.Converted is null ? "" : model.Converted.Name;
                timerTask.SendResult(cards, players, skillName, other);
            }
        }

        /// <summary>
        /// 点击取消键
        /// </summary>
        private void ClickCancel()
        {
            // 取消技能
            if (model.skill != null && timerTask.GivenSkill == "")
            {
                skillArea.Skills.Find(x => x.name == model.skill.Name).ClickSkill();
                return;
            }

            // SetResult
            HideTimer();

            if (timerTask.isWxkj)
            {
                timerTask.SendResult(self.model.position, false);
                timerTask.SendResult(self.model.teammate.position, false);
            }
            else timerTask.SendResult();
        }

        /// <summary>
        /// 点击回合结束键
        /// </summary>
        private void ClickFinishPhase()
        {
            StopAllCoroutines();
            timerTask.SendResult();
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public async void ShowTimer()
        {
            if (!timerTask.isWxkj && !timerTask.isCompete && self.model != timerTask.player) return;
            if (timerTask.isCompete && self.model != timerTask.player0 && self.model != timerTask.player1) return;

            await Util.Instance.WaitFrame(2);

            // while (CardAnime.Instance.InAnimation > 0) await Task.Yield();

            operationArea.SetActive(true);
            hint.text = timerTask.Hint;

            // 初始化进度条和按键

            confirm.gameObject.SetActive(true);
            cancel.gameObject.SetActive(timerTask.Refusable);
            finishPhase.gameObject.SetActive(timerTask.isPerformPhase);

            skillArea.InitSkillArea();
            cardArea.Init();
            cardArea.InitConvertCard();
            destArea.Init();
            equipArea.Init();

            UpdateButtonArea();
            StartCoroutine(StartTimer(timerTask.second));
        }

        /// <summary>
        /// 隐藏进度条
        /// </summary>
        public void HideTimer()
        {
            if (!timerTask.isWxkj && !timerTask.isCompete && self.model != timerTask.player) return;

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
            cancel.interactable = !timerTask.isPerformPhase || model.skill != null;
        }

        public void UseSkill()
        {
            cardArea.Reset();
            destArea.Reset();
            equipArea.Reset();

            skillArea.InitSkillArea();
            cardArea.Init();
            cardArea.InitConvertCard();
            destArea.Init();
            equipArea.Init();

            UpdateButtonArea();
        }
    }
}