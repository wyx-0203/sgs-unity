using UnityEngine;
using UnityEngine.UI;

public class Personal : MonoBehaviour
{
    public Image character;
    public InputField inputField;
    public Button rename;
    public Text winRate;
    public Text game;
    public Text win;
    public Text lose;
    public Button back;

    private async void OnEnable()
    {
        var info = !Global.Instance.IsStandalone ? await Util.GetUserInfo(Global.Instance.userId)
            : new Model.UserInfoResponse { nickname = "standalone", character = "506001" };

        inputField.text = info.nickname;

        float rate = info.win == 0 ? 0 : (float)info.win / (info.win + info.lose);
        winRate.text = rate.ToString();
        game.text = (info.win + info.lose).ToString();
        win.text = info.win.ToString();
        lose.text = info.lose.ToString();

        character.sprite = await SkinAsset.Get(int.Parse(info.character)).GetSeatImage();
    }

    private void OnDisable()
    {
        inputField.interactable = false;
    }

    private void Start()
    {
        back.onClick.AddListener(ClickBack);
        rename.onClick.AddListener(ClickRename);
    }

    private void ClickBack()
    {
        gameObject.SetActive(false);
    }

    private async void ClickRename()
    {
        inputField.interactable = !inputField.interactable;
        if (inputField.interactable) return;

        await WebRequest.GetWithToken($"{Url.DOMAIN_NAME}rename?name={inputField.text}");
    }
}