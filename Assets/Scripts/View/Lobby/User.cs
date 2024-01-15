using UnityEngine;
using UnityEngine.UI;

public class User : MonoBehaviour
{
    // public int ID { get; private set; }
    public Text nickName;
    public Image character;

    public async void Init(Model.UserJson model)
    {
        // ID = model.id;
        nickName.text = model.nickname;

        string url = Url.GENERAL_IMAGE + "Seat/" + model.character + ".png";
        var texture = await WebRequest.GetTexture(url);
        if (texture is null) return;
        character.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}