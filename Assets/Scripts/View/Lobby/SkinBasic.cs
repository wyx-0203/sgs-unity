using UnityEngine;
using UnityEngine.UI;

public class SkinBasic : MonoBehaviour
{
    public Text title;
    public Image image;
    public Toggle toggle;
    public GameObject isSelect;

    private GameCore.Skin model;

    public async void Init(GameCore.Skin model)
    {
        this.model = model;
        toggle.onValueChanged.AddListener(OnValueChanged);
        toggle.group = GeneralDetail.Instance.toggleGroup;

        title.text = model.name;

        string url = Url.GENERAL_IMAGE + "Seat/" + model.id + ".png";
        var texture = await WebRequest.GetTexture(url);
        if (texture is null) return;
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    private void OnValueChanged(bool value)
    {
        if (value)
        {
            isSelect.SetActive(true);
            GeneralDetail.Instance.UpdateSkin(model);
        }
        else isSelect.SetActive(false);
    }
}