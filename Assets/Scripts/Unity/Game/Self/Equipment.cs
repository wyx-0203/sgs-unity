using Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
    public int id => model != null ? model.id : 0;
    public Model.Card model { get; set; }
    public Image cardImage;
    public Image suit;
    public Image weight;
    public Toggle toggle;

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    public void Show(int id)
    {
        model = Model.Card.Find(id);
        name = model.name;

        cardImage.sprite = GameAsset.Instance.equipImage.Get(model.name);
        suit.sprite = GameAsset.Instance.cardSuit.Get(model.suit);
        weight.sprite = model.isBlack ? GameAsset.Instance.cardBlackWeight[model.weight]
            : GameAsset.Instance.cardRedWeight[model.weight];

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        model = null;
    }

    private void OnValueChanged(bool value)
    {
        GetComponent<RectTransform>().anchoredPosition += new Vector2(value ? 20 : -20, 0);
        EquipArea.Instance.OnClickEquipment(this, value);
    }

    /// <summary>
    /// 重置卡牌
    /// </summary>
    public void Reset()
    {
        toggle.isOn = false;
    }
}