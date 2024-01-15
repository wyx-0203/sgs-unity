using Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscardPile : SingletonMono<DiscardPile>
{
    public List<Card> Cards { get; } = new();

    private void Start()
    {
        EventSystem.Instance.AddEvent<FinishPhase>(Clear);
        EventSystem.Instance.AddEvent<FinishOncePlay>(Clear);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<FinishPhase>(Clear);
        EventSystem.Instance.RemoveEvent<FinishOncePlay>(Clear);
    }

    public void Add(Card card)
    {
        Cards.Add(card);
        card.SetParent(transform);
    }

    private async void Clear()
    {
        foreach (var i in Cards) Destroy(i.gameObject, 2);

        await new Delay(2.1f).Run();
        if (this == null) return;
        MoveAll(0.1f);
    }

    private void Clear(FinishPhase finishPhase) => Clear();
    private void Clear(FinishOncePlay finishPlay) => Clear();

    public HorizontalLayoutGroup horizontalLayoutGroup;

    /// <summary>
    /// 弃牌数量达到8时，更新间距
    /// </summary>
    private void UpdateSpacing()
    {
        if (transform.childCount >= 7)
        {
            var spacing = (810 - 121.5f * transform.childCount) / (float)(transform.childCount - 1);
            horizontalLayoutGroup.spacing = spacing;
        }
        else horizontalLayoutGroup.spacing = 0;
    }

    public async void MoveAll(float second)
    {
        UpdateSpacing();

        await Util.WaitFrame();
        foreach (var i in Cards) i.Move(second);
    }
}