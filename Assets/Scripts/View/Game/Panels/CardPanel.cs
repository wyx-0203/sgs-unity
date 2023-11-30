using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPanel : SingletonMono<CardPanel>
{
    public PanelCard selectCard;

    // 进度条
    public Slider slider;

    // 手牌区
    public GameObject handCards;
    // 装备区
    public GameObject equips;
    // 标题
    public Text title;
    public Text hint;
    public Image image;

    protected GameCore.CardPanel model => GameCore.CardPanel.Instance;

    private async void OnEnable()
    {
        hint.text = model.Hint;
        title.text = model.Title;

        StartTimer(model.second);

        foreach (var i in model.cards)
        {
            var display = model.player.team == model.dest.team;
            if (model.dest.handCards.Contains(i)) InitCard(i, handCards, display);
            else InitCard(i, equips);
        }
        UpdateSpacing(handCards.transform.childCount);

        int skinId = model.dest.currentSkin.id;
        string url = Url.GENERAL_IMAGE + "Window/" + skinId + ".png";
        var texture = await WebRequest.GetTexture(url);

        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        image.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        foreach (Transform i in handCards.transform) Destroy(i.gameObject);
        handCards.SetActive(false);

        foreach (Transform i in equips.transform) Destroy(i.gameObject);
        equips.SetActive(false);

        image.gameObject.SetActive(false);
    }

    protected void InitCard(GameCore.Card card, GameObject parent, bool display = true)
    {
        if (!parent.activeSelf) parent.SetActive(true);
        var instance = Card.NewPanelCard(card, display);
        instance.transform.SetParent(parent.transform, false);
    }

    public void OnClickCard()
    {
        if (selectCard != null)
        {
            StopAllCoroutines();
            GameCore.CardPanel.Instance.SendResult(new List<GameCore.Card> { selectCard.model });
        }
    }

    /// <summary>
    /// 开始倒计时
    /// </summary>
    private void StartTimer(int second)
    {
        slider.value = 1;
        StartCoroutine(UpdateTimer(second));
    }

    private IEnumerator UpdateTimer(int second)
    {
        while (slider.value > 0)
        {
            slider.value -= 0.1f / (second - 0.5f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 更新卡牌间距
    /// </summary>
    private void UpdateSpacing(int count)
    {
        float spacing = count >= 8 ? -(count * 121.5f - 850) / (float)(count - 1) - 0.001f : 0;
        handCards.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing, 0);
    }
}