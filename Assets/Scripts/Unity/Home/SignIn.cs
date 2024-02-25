using Model;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SignIn : SingletonMono<SignIn>
{
    // 登录
    public Button signIn;
    // 单机
    [FormerlySerializedAs("singleMode")] public Button standaloneMode;
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
        standaloneMode.onClick.AddListener(ClickStandaloneMode);
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
        var response = (await WebRequest.Post(url, formData)).DeSerialize<SignInResponse>();

        if (response is null)
        {
            errorMessage.text = "连接失败";
            return;
        }

        // 登录成功
        if (response.code == 0)
        {
            // 初始化User信息
            Global.Instance.token = response.token;
            Global.Instance.userId = response.user_id;
            Debug.Log("token: " + response.token);
            Debug.Log("message: " + response.message);
            // 切换到开始游戏界面
            transform.parent.gameObject.SetActive(false);
            SgsStart.Instance.ShowStartPanel();
        }
        // 登录失败
        else errorMessage.text = response.message;

    }

    private void ClickToSignUp()
    {
        gameObject.SetActive(false);
        SignUp.Instance.gameObject.SetActive(true);
    }

    private void ClickStandaloneMode()
    {
        Global.Instance.userId = User.StandaloneId;
        SceneManager.Instance.LoadScene("Lobby");
    }

}