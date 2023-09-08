using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace View
{
    public class CardArea : SingletonMono<CardArea>
    {
        // 手牌区
        public Transform handCardArea;
        // 手牌数
        public Text handCardText;
        // 手牌
        public Dictionary<int, HandCard> handcards = new Dictionary<int, HandCard>();

        private Model.Player self => SgsMain.Instance.self.model;
        private Model.Timer timer => Model.Timer.Instance;

        // 已选卡牌
        private List<Model.Card> SelectedCard => Model.Timer.Instance.temp.cards;
        // 已选技能
        private Model.Skill skill => Model.Timer.Instance.temp.skill;
        // 转化牌
        private Model.Card Converted
        {
            get => Model.Timer.Instance.temp.converted;
            set => Model.Timer.Instance.temp.converted = value;
        }

        public int MaxCount { get; private set; }
        public int MinCount { get; private set; }
        // 是否已设置
        public bool IsValid { get; private set; } = false;


        private void Start()
        {
            // 手牌区
            Model.Timer.Instance.StopTimerView += Reset;

            // 获得牌
            Model.GetCard.ActionView += TeammateAdd;
            Model.GetCard.ActionView += UpdateHandCardText;

            // 失去牌
            Model.LoseCard.ActionView += Remove;
            Model.LoseCard.ActionView += UpdateHandCardText;

            // 改变体力
            Model.UpdateHp.ActionView += UpdateHandCardText;

            // 移动座位
            Model.SgsMain.Instance.MoveSeatView += MoveSeat;
            Model.SgsMain.Instance.MoveSeatView += UpdateHandCardText;
        }

        private void OnDestroy()
        {
            Model.Timer.Instance.StopTimerView -= Reset;

            Model.GetCard.ActionView -= TeammateAdd;
            Model.GetCard.ActionView -= UpdateHandCardText;

            Model.LoseCard.ActionView -= Remove;
            Model.LoseCard.ActionView -= UpdateHandCardText;

            Model.UpdateHp.ActionView -= UpdateHandCardText;

            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;
            Model.SgsMain.Instance.MoveSeatView -= UpdateHandCardText;
        }

        /// <summary>
        /// 添加手牌
        /// </summary>
        public void Add(Card card)
        {
            if (handcards.ContainsKey(card.Id))
            {
                Destroy(handcards[card.Id].gameObject);
                handcards.Remove(card.Id);
            }

            handcards.Add(card.Id, card.handCard);
            card.SetParent(handCardArea);
        }

        /// <summary>
        /// 添加队友手牌 (统帅模式)
        /// </summary>
        public void TeammateAdd(Model.GetCard operation)
        {
            if (operation.player != self.teammate) return;

            foreach (var i in operation.Cards)
            {
                var card = Card.NewHandCard(i);
                card.gameObject.SetActive(false);
                Add(card);
            }
        }

        /// <summary>
        /// 移动座位
        /// </summary>
        public void MoveSeat(Model.Player model)
        {
            foreach (var i in handcards.Values)
            {
                i.gameObject.SetActive(model.HandCards.Contains(i.model));
            }
            MoveAll(0);
        }

        /// <summary>
        /// 移除手牌
        /// </summary>
        public void Remove(Model.LoseCard operation)
        {
            if (!operation.player.isSelf) return;

            foreach (var i in operation.Cards)
            {
                if (!handcards.ContainsKey(i.id)) continue;

                if (!operation.player.teammate.HandCards.Contains(i))
                {
                    Destroy(handcards[i.id].gameObject);
                    handcards.Remove(i.id);
                }

                // 若卡牌还在队友手中，则不移除
                else handcards[i.id].gameObject.SetActive(self != operation.player);
            }
            MoveAll(0.1f);
        }

        /// <summary>
        /// 初始化手牌区
        /// </summary>
        public void Init()
        {
            // 可选卡牌数量
            MaxCount = timer.maxCard;
            MinCount = timer.minCard;

            // 无懈可击
            if (timer is Model.WxkjTimer)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(i.name == "无懈可击");
                MoveAll(0);
            }

            Update_();
        }

        /// <summary>
        /// 重置手牌区（进度条结束时调用）
        /// </summary>
        public void Reset()
        {
            if (!timer.players.Contains(self)) return;

            // 重置手牌状态
            foreach (var card in handcards.Values) if (card.gameObject.activeSelf) card.Reset();
            if (timer is Model.WxkjTimer)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(self.HandCards.Contains(i.model));
                MoveAll(0);

            }
            if (ConvertCards.Count > 0)
            {
                foreach (var i in ConvertCards.Values) Destroy(i.gameObject);
                ConvertCards.Clear();
                MoveAll(0.1f);
            }

            IsValid = false;
            Converted = null;
        }

        /// <summary>
        /// 更新手牌区
        /// </summary>
        public void Update_()
        {
            // 若已选中手牌数量超出范围，取消第一张选中的手牌
            while (SelectedCard.Count > MaxCount)
            {
                if (handcards.ContainsKey(SelectedCard[0].id)) handcards[SelectedCard[0].id].Unselect();
                else EquipArea.Instance.Equips.Values.First(x => x.model == SelectedCard[0]).Unselect();
            }

            IsValid = SelectedCard.Count >= MinCount;

            // 转化牌
            if (IsValid && skill != null && skill is Model.Converted)
            {
                Converted = (skill as Model.Converted).Convert(SelectedCard);
            }
            else Converted = null;

            // 判断每张卡牌是否可选
            if (MaxCount > 0)
            {
                foreach (var i in handcards.Values)
                {
                    if (!i.gameObject.activeSelf) continue;
                    i.button.interactable = timer.isValidCard(i.model);
                }
            }

            // 对已禁用的手牌设置阴影
            foreach (var card in handcards.Values) if (card.gameObject.activeSelf) card.SetShadow();
        }


        public Dictionary<string, HandCard> ConvertCards { get; set; } = new Dictionary<string, HandCard>();
        public bool ConvertIsValid { get; private set; }

        public void InitConvertCard()
        {
            ConvertIsValid = timer.multiConvert.Count == 0;
            if (!IsValid || ConvertIsValid) return;

            foreach (var i in handcards.Values) i.button.interactable = false;
            foreach (var i in timer.multiConvert)
            {
                var card = Card.NewHandCard(i);
                card.SetParent(handCardArea);
                ConvertCards.Add(i.name, card.handCard);
            }
            foreach (var i in ConvertCards.Values)
            {
                i.button.interactable = timer.isValidCard(i.model);
            }

            foreach (var i in ConvertCards.Values) if (i.gameObject.activeSelf) i.SetShadow();
            MoveAll(0);
        }

        public void UpdateConvertCard()
        {
            ConvertIsValid = Converted != null;
        }

        public GridLayoutGroup gridLayoutGroup;

        /// <summary>
        /// 更新卡牌间距
        /// </summary>
        private void UpdateSpacing()
        {
            int count = SgsMain.Instance.self.model.HandCardCount + ConvertCards.Count;

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 8)
            {
                gridLayoutGroup.spacing = new Vector2(0, 0);
            }
            else
            {
                float spacing = -(count * 121.5f - 950) / (float)(count - 1) - 0.001f;
                gridLayoutGroup.spacing = new Vector2(spacing, 0);
            }
        }

        public async void MoveAll(float second)
        {
            UpdateSpacing();
            await Util.Instance.WaitFrame();
            foreach (var i in handcards.Values) i.card.Move(second);
            if (IsValid && !ConvertIsValid) foreach (var i in ConvertCards.Values) i.card.Move(0);
        }


        /// <summary>
        /// 更新手牌数与手牌上限信息
        /// </summary>
        private void UpdateHandCardText()
        {
            handCardText.text = self.HandCardCount.ToString() + "/" + self.HandCardLimit.ToString();
        }

        private void UpdateHandCardText(Model.Player player)
        {
            if (self != player) return;
            UpdateHandCardText();
        }

        private void UpdateHandCardText(Model.GetCard operation)
        {
            if (self != operation.player) return;
            UpdateHandCardText();
        }

        private void UpdateHandCardText(Model.LoseCard operation)
        {
            if (self != operation.player) return;
            UpdateHandCardText();
        }

        private void UpdateHandCardText(Model.UpdateHp operation)
        {
            if (self != operation.player) return;
            UpdateHandCardText();
        }
    }
}