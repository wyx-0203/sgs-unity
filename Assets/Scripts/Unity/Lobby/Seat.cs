using UnityEngine;
using UnityEngine.UI;

public class Seat : MonoBehaviour
{
    public GameObject user;
    public int userId { get; set; }
    public GameObject already;
    public GameObject owner;
    public Text nickName;
    public Image character;

    // private Model.User model;

    public async void AddUser(Model.User model)
    {
        // if (model is null)
        // {
        //     user.gameObject.SetActive(false);
        //     return;
        // }
        // this.model = model;

        // user.gameObject.SetActive(true);
        // user.Init(model);
        userId = model.id;

        already.SetActive(model.already);
        owner.SetActive(Room.Instance.IsOwner(userId));

        // nickName.gameObject.SetActive(true);

        user.SetActive(true);
        nickName.text = "nickname";
        character.sprite = await SkinAsset.Get(107501).GetSeatImage();

        // string url = Url.GENERAL_IMAGE + "Seat/" + model.character + ".png";
        // var texture = await WebRequest.GetTexture(url);
        // if (texture is null) return;
        // character.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        // UpdateStatus();
    }

    public void RemovePlayer()
    {
        // model = null;
        userId = 0;
        already.SetActive(false);
        owner.SetActive(false);
        user.SetActive(false);
    }

    // public void UpdateStatus()
    // {
    //     already.SetActive(model.already);
    //     owner.SetActive(model.owner);
    // }

    // public void SetAready(bool value)=>already.SetActive(value);
    // public void SetOwner(bool value)=>owner.SetActive(value);
}