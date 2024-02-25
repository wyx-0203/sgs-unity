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

    public async void AddUser(Model.User model)
    {
        userId = model.id;

        already.SetActive(model.already);
        owner.SetActive(Room.Instance.IsOwner(userId));

        var userInfo = await Util.GetUserInfo(model.id);
        nickName.text = userInfo.nickname;
        character.sprite = await SkinAsset.Get(int.Parse(userInfo.character)).GetSeatImage();
        user.SetActive(true);
    }

    public void RemovePlayer()
    {
        userId = 0;
        already.SetActive(false);
        owner.SetActive(false);
        user.SetActive(false);
    }
}