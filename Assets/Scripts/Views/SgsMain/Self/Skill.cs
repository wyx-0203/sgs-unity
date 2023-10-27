using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Skill : MonoBehaviour
    {
        public Button button;
        public Text text;
        public GameObject effect;

        public Model.Skill model { get; set; }
        public bool IsSelected { get; private set; }
        public OperationArea operationArea => OperationArea.Instance;
        public SkillArea skillArea => SkillArea.Instance;

        public void Init(Model.Skill model)
        {
            this.model = model;
            name = model.name;
            text.text = model.name;
            button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// 点击技能
        /// </summary>
        public void OnClick()
        {
            if (model is Model.Triggered) return;
            if (!IsSelected) Select();
            else Unselect();
            operationArea.UseSkill();
        }

        /// <summary>
        /// 选中技能
        /// </summary>
        private void Select()
        {
            if (IsSelected) return;

            IsSelected = true;
            effect.SetActive(true);
            Model.Timer.Instance.temp.skill = model;
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        private void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            effect.SetActive(false);
            Model.Timer.Instance.temp.skill = null;
        }

        /// <summary>
        /// 重置技能
        /// </summary>
        public void Reset()
        {
            button.interactable = false;
            Unselect();
        }
    }
}