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

        private Model.Operation model => Model.Operation.Instance;
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

            var cards = model.Cards.Select(x => x.Id).Union(model.Equips.Select(x => x.Id)).ToList();
            var players = model.Dests.Select(x => x.position).ToList();
            var skill = model.skill != null ? model.skill.Name : "";

            if (timer is Model.WxkjTimer)
            {
                bool isSelf = self.model.HandCards.Contains(Model.CardPile.Instance.cards[cards[0]]);
                (timer as Model.WxkjTimer).SendResult((isSelf ? self.model : self.model.teammate).position, true, cards);
            }
            else if (timer is Model.CompeteTimer)
            {
                HideTimer();
                (timer as Model.CompeteTimer).SendResult(self.model.position, true, cards);
            }
            else
            {
                string other = model.Converted is null ? "" : model.Converted.Name;
                timer.SendResult(cards, players, skill, other);
            }
        }

        /// <summary>
        /// 点击取消键
        /// </summary>
        private void ClickCancel()
        {
            // 取消技能
            if (model.skill != null && timer.GivenSkill == "")
            {
                skillArea.Skills.Find(x => x.name == model.skill.Name).ClickSkill();
                return;
            }

            // SetResult

            if (timer is Model.WxkjTimer)
            {
                HideTimer();
                (timer as Model.WxkjTimer).SendResult(self.model.position, false);
                if (self.model.teammate.IsAlive) (timer as Model.WxkjTimer).SendResult(self.model.teammate.position, false);
            }
            else timer.SendResult();
        }

        /// <summary>
        /// 点击回合结束键
        /// </summary>
        private void ClickFinishPhase()
        {
            StopAllCoroutines();
            timer.SendResult();
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public async void ShowTimer()
        {
            if (!timer.players.Contains(self.model)) return;
            await Util.Instance.WaitFrame(2);

            operationArea.SetActive(true);
            hint.text = timer.Hint;

            // 初始化进度条和按键

            confirm.gameObject.SetActive(true);
            cancel.gameObject.SetActive(timer.Refusable);
            finishPhase.gameObject.SetActive(timer.isPerformPhase);

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
            cancel.interactable = !timer.isPerformPhase || model.skill != null;
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