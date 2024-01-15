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

    // private async void OnEnable()
    // {
    //     var msg = await WebRequest.Get(Url.DOMAIN_NAME + "getUserInfo?id=" + GameCore.Self.Instance.UserId);
    //     var json = JsonUtility.FromJson<Model.UserInfoResponse>(msg);

    //     inputField.text = json.nickname;

    //     float rate = json.win == 0 ? 0 : (float)json.win / (json.win + json.lose);
    //     winRate.text = rate.ToString();
    //     game.text = (json.win + json.lose).ToString();
    //     win.text = json.win.ToString();
    //     lose.text = json.lose.ToString();

    //     string url = Url.GENERAL_IMAGE + "Seat/" + json.character + ".png";
    //     var texture = await WebRequest.GetTexture(url);
    //     if (texture is null) return;
    //     character.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    // }

    private void OnDisable()
    {
        inputField.interactable = false;
    }

    private void Start()
    {
        back.onClick.AddListener(ClickBack);
        rename.onClick.AddListener(ClickRename);

        // #if UNITY_WEBGL
        //             // webgl中文输入支持
        //             inputField.gameObject.AddComponent<WebGLSupport.WebGLInput>();
        // #endif
    }

    private void ClickBack()
    {
        gameObject.SetActive(false);
    }

    private async void ClickRename()
    {
        inputField.interactable = !inputField.interactable;
        if (inputField.interactable) return;

        Debug.Log("rename");
        await WebRequest.GetWithToken(Url.DOMAIN_NAME + "rename?name=" + inputField.text);
    }
}