using UnityEngine;
using UnityEngine.UI;

public class SignUp : SingletonMono<SignUp>
{
    // 注册按键
    public Button signUp;
    // 返回登录
    public Button toSignIn;

    // 用户名
    public InputField username;
    // 密码
    public InputField password;
    // 确认密码
    public InputField confirmPassword;
    // 错误信息
    public Text errorMessage;

    private void Start()
    {
        signUp.onClick.AddListener(ClickSignUp);
        toSignIn.onClick.AddListener(ClickToSignIn);
    }

    private async void ClickSignUp()
    {
        errorMessage.text = "";

        if (username.text == "" || username.text == null)
        {
            errorMessage.text = "用户名不能为空！";
            return;
        }
        if (password.text == "")
        {
            errorMessage.text = "密码不能为空！";
            return;
        }
        if (password.text != confirmPassword.text)
        {
            errorMessage.text = "两次密码不一致！";
            return;
        }

        // 发送请求
        string url = Url.DOMAIN_NAME + "signup";
        WWWForm formData = new WWWForm();
        formData.AddField("username", username.text);
        formData.AddField("password", password.text);
        var response = JsonUtility.FromJson<Model.HttpResponse>(await WebRequest.Post(url, formData));

        // 注册成功
        if (response.code == 0)
        {
            ClickToSignIn();
            SignIn.Instance.username.text = username.text;
            SignIn.Instance.password.text = password.text;
        }
        // 注册失败
        else errorMessage.text = response.message;
    }

    private void ClickToSignIn()
    {
        gameObject.SetActive(false);
        SignIn.Instance.gameObject.SetActive(true);
    }
}