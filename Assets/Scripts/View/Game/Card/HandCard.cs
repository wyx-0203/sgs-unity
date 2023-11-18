using UnityEngine;
using UnityEngine.UI;

public class HandCard : MonoBehaviour
{
    public Card card;
    public Toggle toggle;

    public GameCore.Card model { get; private set; }

    protected bool refresh = true;
    private ColorBlock colorBlock;

    public void Init()
    {
        card = GetComponent<Card>();
        model = card.model;

        toggle = gameObject.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        colorBlock = toggle.colors;
    }

    protected virtual void OnValueChanged(bool value)
    {
        GetComponent<RectTransform>().anchoredPosition += new Vector2(0, value ? 20 : -20);
        if (value) GameCore.Timer.Instance.temp.cards.Add(model);
        else GameCore.Timer.Instance.temp.cards.Remove(model);

        if (refresh)
        {
            CardArea.Instance.Update_();
            DestArea.Instance.Reset();
            DestArea.Instance.OnStartPlay();
            OperationArea.Instance.UpdateButtonArea();
        }
    }

    /// <summary>
    /// 取消选中
    /// </summary>
    public void Unselect()
    {
        refresh = false;
        toggle.isOn = false;
        refresh = true;
    }

    /// <summary>
    /// 设置阴影
    /// </summary>
    public void SetDisabledColor()
    {
        colorBlock.disabledColor = new(0.5f, 0.5f, 0.5f);
        toggle.colors = colorBlock;
    }


    /// <summary>
    /// 重置卡牌
    /// </summary>
    public void Reset()
    {
        toggle.interactable = false;
        colorBlock.disabledColor = Color.white;
        toggle.colors = colorBlock;
        Unselect();
    }
}