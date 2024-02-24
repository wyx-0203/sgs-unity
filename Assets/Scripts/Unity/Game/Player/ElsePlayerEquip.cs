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

        cardImage.sprite = GameAsset.Instance.equipImage.Get(model.name);
        suit.sprite = GameAsset.Instance.equipSuit.Get(model.suit);
        if (model.isBlack) weight.sprite = GameAsset.Instance.equipBlackWeight[model.weight];
        else weight.sprite = GameAsset.Instance.equipRedWeight[model.weight];

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        model = null;
    }
}