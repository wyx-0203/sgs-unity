using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Dest : MonoBehaviour
    {
        // 按键
        public Button button;
        // 被选中边框
        public Image border;
        // 武将图片(设置阴影)
        public Image skin;
        // 是否被选中
        public bool IsSelected { get; private set; }

        private Player player;
        public Model.Player model => player.model;


        private void Start()
        {
            player = GetComponent<Player>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            // 选中目标角色
            if (!IsSelected) Select();
            else Unselect();

            DestArea.Instance.Update_();
            OperationArea.Instance.UpdateButtonArea();
        }

        /// <summary>
        /// 选中目标角色
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;
            IsSelected = true;
            border.gameObject.SetActive(true);
            Model.Timer.Instance.temp.dests.Add(model);
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            border.gameObject.SetActive(false);
            Model.Timer.Instance.temp.dests.Remove(model);
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public void AddShadow()
        {
            if (!button.interactable && !IsSelected && model.IsAlive) skin.color = new Color(0.5f, 0.5f, 0.5f);
            else skin.color = new Color(1, 1, 1);
        }

        /// <summary>
        /// 重置玩家按键
        /// </summary>
        public void Reset()
        {
            button.interactable = false;
            Unselect();
            skin.color = new Color(1, 1, 1);
        }
    }
}