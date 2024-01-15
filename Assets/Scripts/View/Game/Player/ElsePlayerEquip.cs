using UnityEngine;
using UnityEngine.UI;

public class ElsePlayerEquip : MonoBehaviour
{
    public int id => model != null ? model.id : -1;
    public Model.Card model { get; set; }

    public Image cardImage;
    public Image suit;
    public Image weight;

    // private Sprites sprites => Sprites.Instance;

    public void Show(int id)
    {
        model = Model.Card.Find(id);
        name = model.name;

        cardImage.sprite = GameAssets.Instance.equipImage.Get(model.name);
        suit.sprite = GameAssets.Instance.equipSuit.Get(model.suit);
        if (model.isBlack) weight.sprite = GameAssets.Instance.equipBlackWeight[model.weight];
        else weight.sprite = GameAssets.Instance.equipRedWeight[model.weight];

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        model = null;
    }
}