using UnityEngine;
using UnityEngine.UI;

public class SignIn : SingletonMono<SignIn>
{
    // 登录
    public Button signIn;
    // 单机
    public Button singleMode;
    // 前往注册
    public Button toSignUp;

    // 用户名
    public InputField username;
    // 密码
    public InputField password;
    // 错误信息
    public Text errorMessage;

    private void Start()
    {
        signIn.onClick.AddListener(ClickSignIn);
        toSignUp.onClick.AddListener(ClickToSignUp);
        singleMode.onClick.AddListener(ClickSingleMode);
    }

    private async void ClickSignIn()
    {
        errorMessage.text = "";

        if (username.text == "" || username.text is null)
        {
            errorMessage.text = "用户名不能为空";
            return;
        }
        if (password.text == "")
        {
            errorMessage.text = "密码不能为空";
            return;
        }

        // 发送请求
        string url = Url.DOMAIN_NAME + "signin";
        var formData = new WWWForm();
        formData.AddField("username", username.text);
        formData.AddField("password", password.text);
        var response = JsonUtility.FromJson<SignInResponse>(await WebRequest.Post(url, formData));

        if (response is null)
        {
            errorMessage.text = "连接失败";
            return;
        }

        // 登录成功
        if (response.code == 0)
        {
            // 初始化User信息
            GameCore.Self.Instance.Init(response);
            // Debug.Log("token: " + GameCore.Self.Instance.Token);
            // 切换到开始游戏界面
            SgsStart.Instance.ShowStartPanel();
        }
        // 登录失败
        else errorMessage.text = response.message;

    }

    private void ClickToSignUp()
    {
        gameObject.SetActive(false);
        transform.parent.Find("注册").gameObject.SetActive(true);
    }

    private void ClickSingleMode()
    {
        SceneManager.Instance.LoadScene("Game");
    }

}