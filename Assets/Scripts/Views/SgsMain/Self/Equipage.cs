using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Equipage : MonoBehaviour
    {
        public int Id { get; private set; }
        public Image cardImage;
        public Image suit;
        public Image weight;

        public Button button;
        public bool IsSelected { get; private set; }
        public bool UseSkill { get; private set; }

        public EquipArea equipArea => EquipArea.Instance;
        public OperationArea operationArea => OperationArea.Instance;
        public Model.Card model => Model.CardPile.Instance.cards[Id];
        private Model.Skill skill { get => Model.Operation.Instance.skill; set => Model.Operation.Instance.skill = value; }

        private void Start()
        {
            // button.interactable = false;
            button.onClick.AddListener(ClickCard);
        }

        public void Init(Model.Equipage card)
        {
            Id = card.Id;
            name = card.Name;

            var sprites = Sprites.Instance;
            cardImage.sprite = sprites.equipImage[name];
            suit.sprite = sprites.cardSuit[card.Suit];
            if (card.Suit == "黑桃" || card.Suit == "草花") weight.sprite = sprites.blackWeight[card.Weight];
            else weight.sprite = sprites.redWeight[card.Weight];
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickCard()
        {
            // 可发动丈八蛇矛
            if (name == "丈八蛇矛" && Model.Timer.Instance.IsValidCard(Model.Card.Convert<Model.杀>()))
            {
                // var skill = SkillArea.Instance.SelectedSkill;
                if (skill is null)
                {
                    skill = (model as Model.丈八蛇矛).skill;
                    operationArea.UseSkill();
                    Use();
                }
                else if (skill == (model as Model.丈八蛇矛).skill)
                {
                    skill = null;
                    operationArea.UseSkill();
                    Cancel();
                }
            }
            else
            {
                // 选中卡牌
                if (!IsSelected) Select();
                else Unselect();
                CardArea.Instance.Update_();
                DestArea.Instance.Init();
                OperationArea.Instance.UpdateButtonArea();
            }

        }

        /// <summary>
        /// 选中卡牌
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;

            IsSelected = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(20, 0);
            // equipArea.SelectedCard.Add(this);
            Model.Operation.Instance.Equips.Add(model);
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(20, 0);
            // equipArea.SelectedCard.Remove(this);
            Model.Operation.Instance.Equips.Remove(model);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void ResetCard()
        {
            button.interactable = false;
            Unselect();
            Cancel();
        }

        /// <summary>
        /// 发动技能
        /// </summary>
        public void Use()
        {
            if (UseSkill) return;

            UseSkill = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(20, 0);
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            if (!UseSkill) return;
            UseSkill = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(20, 0);
        }
    }
}
