using System.Threading.Tasks;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : SingletonMono<Lobby>
{
    // 快速加入
    public Button quickJoin;

    // 统帅模式
    public Toggle hlcs;
    // 欢乐模式
    public Toggle tssj;

    private readonly Mode mode = Mode._3V3;
    public Image modeBg;
    public Sprite hlcsImg;
    public Sprite tssjImg;

    // 大厅
    public GameObject lobby;
    // 武将列表
    public GameObject generalList;
    // 个人信息
    public GameObject personalPanel;

    // 武将按钮
    public Button general;
    public Button personal;

    private async void Start()
    {
        quickJoin.onClick.AddListener(ClickQuickJoin);
        general.onClick.AddListener(ClickGeneral);
        personal.onClick.AddListener(ClickPersonal);

        EventSystem.Instance.AddEvent<JoinRoom>(OnJoinRoom);
        EventSystem.Instance.AddEvent<RemoveUser>(OnRemoveUser);
        EventSystem.Instance.AddEvent<StartGame>(OnStartGame);

        // Tssj(true);

        // BGM.Instance.Load(Url.AUDIO + "bgm/outbgm_2.mp3");
        // WebSocket.Instance.Connect();

        // if (model.players != null && model.players.Count > 0)
        // {
        //     JoinRoom();
        // }
        Connection.Instance.CheckRoom();

#if UNITY_EDITOR
        if (!Global.Instance.IsStandalone) return;

        // 发送请求
        string url = Url.DOMAIN_NAME + "signin";
        var formData = new WWWForm();
        formData.AddField("username", "1");
        formData.AddField("password", "1");
        var response = (await WebRequest.Post(url, formData)).DeSerialize<SignInResponse>();

        // 登录成功
        if (response.code == 0)
        {
            // 初始化User信息
            Global.Instance.token = response.token;
            Global.Instance.userId = response.user_id;
            Debug.Log("token: " + response.token);
            Debug.Log("id: " + response.user_id);
        }

        // Model.General.Init(await WebRequest.Get(Url.JSON + "general.json"));
        // Model.Card.Init(await WebRequest.Get(Url.JSON + "card.json"));
        // await SkinAsset.Init();
        // await SkillAsset.Init();
#else
        await Task.CompletedTask;
#endif
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<JoinRoom>(OnJoinRoom);
        EventSystem.Instance.RemoveEvent<RemoveUser>(OnRemoveUser);
        EventSystem.Instance.RemoveEvent<StartGame>(OnStartGame);
    }

    private async void ClickQuickJoin() => await Connection.Instance.JoinRoom(mode);

    public void OnJoinRoom(JoinRoom joinRoom)
    {
        lobby.SetActive(false);
        Room.Instance.Show(joinRoom);
    }

    public void OnRemoveUser(RemoveUser removeUser)
    {
        if (removeUser.userId != Global.Instance.userId) return;
        lobby.SetActive(true);
    }

    public async void OnStartGame(StartGame startGame)
    {
        await EventSystem.Instance.Lock();
        SceneManager.Instance.LoadScene("Game");
    }

    // private void ClickBack()
    // {
    //     currentPanel.SetActive(false);
    //     lobby
    // }

    private void ClickGeneral()
    {
        lobby.SetActive(false);
        generalList.SetActive(true);
    }

    public void ShowLobby()
    {
        lobby.SetActive(true);
    }

    private void ClickPersonal()
    {
        personalPanel.SetActive(true);
    }
}