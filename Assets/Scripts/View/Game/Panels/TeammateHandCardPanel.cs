using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TeammateHandCardPanel : SingletonMono<TeammateHandCardPanel>
{
    public GameObject handCardArea;
    public Image image;
    private List<Card> handcards = new();

    protected override void Awake()
    {
        base.Awake();

        GameCore.GetCard.ActionView += AddHandCard;
        GameCore.LoseCard.ActionView += RemoveHandCard;
    }

    private void OnDestroy()
    {
        GameCore.GetCard.ActionView -= AddHandCard;
        GameCore.LoseCard.ActionView -= RemoveHandCard;
    }

    public async void Show(GameCore.Player player)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        foreach (var i in player.handCards)
        {
            if (handcards.Find(x => x.model == i)) continue;
            var card = Card.New(i, true);
            card.transform.SetParent(handCardArea.transform, false);
            handcards.Add(card);
        }

        foreach (var i in handcards) i.gameObject.SetActive(player.handCards.Contains(i.model));
        UpdateSpacing();

        int skinId = player.currentSkin.id;
        string url = Url.GENERAL_IMAGE + "Window/" + skinId + ".png";
        var texture = await WebRequest.GetTexture(url);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
    }

    private void AddHandCard(GameCore.GetCard operation)
    {
        if (!operation.player.isSelf) return;

        // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
        foreach (var i in operation.Cards)
        {
            if (handcards.Find(x => x.model == i) is Card card)
            {
                card.transform.SetAsLastSibling();
                continue;
            }

            card = Card.New(i, true);
            card.transform.SetParent(handCardArea.transform, false);
            handcards.Add(card);

        }
    }

    private void RemoveHandCard(GameCore.LoseCard operation)
    {
        if (!operation.player.isSelf) return;

        foreach (var i in operation.Cards)
        {
            if (handcards.Find(x => x.model == i) is not Card card) continue;

            // 若卡牌还在队友手中，则不移除
            if (card.model.isHandCard && card.model.src.isSelf) card.gameObject.SetActive(false);

            else
            {
                handcards.Remove(card);
                Destroy(card.gameObject);
            }
        }
    }

    private void UpdateSpacing()
    {
        int count = handcards.Where(x => x.gameObject.activeSelf).Count();
        float spacing = count < 7 ? 0 : -(count * 121.5f - 820) / (float)(count - 1) - 0.001f;
        handCardArea.GetComponent<HorizontalLayoutGroup>().spacing = spacing;
    }
}