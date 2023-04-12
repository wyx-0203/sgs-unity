using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class 队友手牌 : SingletonMono<队友手牌>
    {
        public GameObject handCardArea;
        public Image image;
        public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
        private Model.Player self => SgsMain.Instance.self.model;

        protected override void Awake()
        {
            base.Awake();

            foreach (var i in self.HandCards.Union(self.teammate.HandCards))
            {
                var card = Card.New(i, true);
                card.transform.SetParent(handCardArea.transform, false);
                handcards.Add(i.Id, card);
            }

            Model.GetCard.ActionView += AddHandCard;
            Model.LoseCard.ActionView += RemoveHandCard;
        }

        private void OnDestroy()
        {
            Model.GetCard.ActionView -= AddHandCard;
            Model.LoseCard.ActionView -= RemoveHandCard;
        }

        private async void OnEnable()
        {
            transform.SetAsLastSibling();
            int count = 0;
            foreach (var i in handcards.Values)
            {
                if (self.teammate.HandCards.Contains(i.model))
                {
                    i.gameObject.SetActive(true);
                    count++;
                }
                else i.gameObject.SetActive(false);
            }
            handCardArea.GetComponent<HorizontalLayoutGroup>().spacing = UpdateSpacing(count);

            int skinId = self.teammate.currentSkin.id;
            string url = Url.GENERAL_IMAGE + "Window/" + skinId + ".png";
            var texture = await WebRequest.GetTexture(url);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
        }

        private void AddHandCard(Model.GetCard operation)
        {
            if (!operation.player.isSelf) return;

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            foreach (var i in operation.Cards)
            {
                if (handcards.ContainsKey(i.Id))
                {
                    handcards[i.Id].transform.SetAsLastSibling();
                    continue;
                }

                var card = Card.New(i, true);
                card.transform.SetParent(handCardArea.transform, false);
                handcards.Add(i.Id, card);
            }
        }

        private void RemoveHandCard(Model.LoseCard operation)
        {
            if (!operation.player.isSelf) return;

            foreach (var i in operation.Cards)
            {
                if (!handcards.ContainsKey(i.Id)) continue;
                if (!operation.player.teammate.HandCards.Contains(i))
                {
                    Destroy(handcards[i.Id].gameObject);
                    handcards.Remove(i.Id);
                }
                else handcards[i.Id].gameObject.SetActive(self != operation.player);
            }
        }

        private float UpdateSpacing(int count)
        {
            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 7) return 0;

            return -(count * 121.5f - 820) / (float)(count - 1) - 0.001f;
        }
    }
}
