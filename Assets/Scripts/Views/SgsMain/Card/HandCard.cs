using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class HandCard : MonoBehaviour
    {
        public Card card;
        public Button button;

        // 是否被选中
        public bool IsSelected { get; private set; }

        // 手牌区
        private CardArea cardArea => CardArea.Instance;

        public Model.Card model { get; private set; }

        private Model.Card Converted
        {
            get => Model.Timer.Instance.temp.converted;
            set => Model.Timer.Instance.temp.converted = value;
        }

        public void Init()
        {
            card = GetComponent<Card>();
            model = card.model;

            button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(ClickCard);
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickCard()
        {
            // 选中卡牌
            if (!IsSelected) Select();
            else Unselect();

            if (!model.IsConvert) cardArea.Update_();
            else cardArea.UpdateConvertCard();

            DestArea.Instance.Reset();
            DestArea.Instance.OnStartPlay();
            OperationArea.Instance.UpdateButtonArea();
        }

        /// <summary>
        /// 选中卡牌
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;
            IsSelected = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 20);
            if (!model.IsConvert) Model.Timer.Instance.temp.cards.Add(model);
            else
            {
                if (Converted != null) cardArea.ConvertCards[Converted.name].Unselect();
                Converted = model;
            }
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 20);
            if (!model.IsConvert) Model.Timer.Instance.temp.cards.Remove(model);
            else Converted = null;
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public void SetShadow()
        {
            card.shadow.gameObject.SetActive(!button.interactable);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void Reset()
        {
            button.interactable = false;
            Unselect();
            card.shadow.gameObject.SetActive(false);
        }
    }
}