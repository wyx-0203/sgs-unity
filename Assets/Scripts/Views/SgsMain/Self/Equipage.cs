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
        public Model.Card model { get; set; }
        private Model.Skill skill { get => Model.Timer.Instance.temp.skill; set => Model.Timer.Instance.temp.skill = value; }

        private void Start()
        {
            // button.interactable = false;
            button.onClick.AddListener(OnClick);
        }

        public void Init(Model.Equipment card)
        {
            Id = card.id;
            name = card.name;
            model = Model.CardPile.Instance.cards[Id];

            var sprites = Sprites.Instance;
            cardImage.sprite = sprites.equipImage[name];
            suit.sprite = sprites.cardSuit[card.suit];
            weight.sprite = card.isBlack ? sprites.blackWeight[card.weight] : sprites.redWeight[card.weight];
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void OnClick()
        {
            // 可发动丈八蛇矛
            if (model is Model.丈八蛇矛 zbsm && (zbsm.skill.IsValid || skill == zbsm.skill))
            {
                // var skill = SkillArea.Instance.SelectedSkill;
                if (skill is null)
                {
                    skill = zbsm.skill;
                    operationArea.UseSkill();
                    Use();
                }
                else if (skill == zbsm.skill)
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
                DestArea.Instance.OnStartPlay();
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
            Model.Timer.Instance.temp.cards.Add(model);
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
            Model.Timer.Instance.temp.cards.Remove(model);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void Reset()
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
