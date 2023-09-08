using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

        private Model.Decision model => timer.temp;
        private Model.Timer timer => Model.Timer.Instance;

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

        /// <summary>
        /// 点击确定键
        /// </summary>
        private void ClickConfirm()
        {
            StopAllCoroutines();

            timer.temp.action = true;
            timer.temp.src = timer.temp.cards.FirstOrDefault()?.Src;
            timer.SendDecision();
        }

        /// <summary>
        /// 点击取消键
        /// </summary>
        private void ClickCancel()
        {
            // 取消技能
            if (model.skill != null && timer.givenSkill is null)
            {
                skillArea.Skills.Find(x => x.name == model.skill.Name).ClickSkill();
                return;
            }

            // SetResult

            if (timer is Model.WxkjTimer wxkjTimer)
            {
                HideTimer();
                foreach (var i in Model.SgsMain.Instance.AlivePlayers.Where(x => x.team == self.model.team))
                {
                    wxkjTimer.temp.src = i;
                    wxkjTimer.SendDecision();
                }
            }
            else if (DebugAI) timer.SendDecision(timer.AIDecision());
            else timer.SendDecision();
        }

        public bool DebugAI;

        /// <summary>
        /// 点击回合结束键
        /// </summary>
        private void ClickFinishPhase()
        {
            StopAllCoroutines();
            if (DebugAI) timer.SendDecision(timer.AIDecision());
            else timer.SendDecision();
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public async void ShowTimer()
        {
            if (!timer.players.Contains(self.model)) return;
            await Util.Instance.WaitFrame(2);

            operationArea.SetActive(true);
            hint.text = timer.hint;

            // 初始化进度条和按键

            confirm.gameObject.SetActive(true);
            cancel.gameObject.SetActive(timer.refusable);
            finishPhase.gameObject.SetActive(timer.isPlayPhase);

            skillArea.InitSkillArea();
            cardArea.Init();
            cardArea.InitConvertCard();
            destArea.Init();
            equipArea.Init();

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
            cancel.interactable = !timer.isPlayPhase || model.skill != null;
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