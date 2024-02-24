using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardPanel : SingletonMono<CardPanel>
{
    private PanelCard selectCard;

    // 进度条
    public Slider slider;

    // 手牌区
    public Transform handCards;
    // 装备区
    public Transform equipments;
    // 标题
    public Text title;
    public Text hint;
    public Image image;

    public void Show(Model.CardPanelQuery cpr)
    {
        gameObject.SetActive(true);
        title.text = cpr.title;
        hint.text = cpr.hint;
        var player = Player.Find(cpr.player);
        var dest = Player.Find(cpr.dest);

        StartTimer(cpr.second);

        foreach (var i in cpr.handCards)
        {
            bool display = player.model.team == dest.model.team;
            InitCard(i, handCards, display);
        }
        UpdateSpacing(cpr.handCards.Count);

        foreach (var i in cpr.equipments.Union(cpr.judgeCards))
        {
            InitCard(i, equipments);
        }

        // image.sprite = await dest.skin.asset.GetWindowImage();
        // image.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        foreach (Transform i in handCards.transform) Destroy(i.gameObject);
        handCards.gameObject.SetActive(false);

        foreach (Transform i in equipments.transform) Destroy(i.gameObject);
        equipments.gameObject.SetActive(false);

        image.gameObject.SetActive(false);
    }

    protected void InitCard(int cardId, Transform parent, bool display = true)
    {
        parent.gameObject.SetActive(true);
        var instance = Card.NewPanelCard(cardId, display);
        instance.transform.SetParent(parent, false);
    }

    public void OnClickCard(PanelCard panelCard)
    {
        selectCard = panelCard;
        if (selectCard != null)
        {
            StopAllCoroutines();
            EventSystem.Instance.SendToServer(new Model.CardDecision
            {
                player = Game.Instance.firstPerson.model.index,
                cards = new List<int> { selectCard.id }
            });
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