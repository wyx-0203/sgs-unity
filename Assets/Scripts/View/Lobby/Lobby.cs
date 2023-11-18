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

    private string mode = "2v2";
    public Image modeBg;
    public Sprite hlcsImg;
    public Sprite tssjImg;

    // 大厅
    public GameObject lobby;
    // 房间
    public GameObject room;
    // 武将列表
    public GameObject generalList;
    public GameObject personalPanel;

    // 武将按钮
    public Button general;
    public Button personal;

    private GameCore.Room model => GameCore.Room.Instance;

    private void Start()
    {
        model.JoinRoomView += JoinRoom;
        model.ExitRoomView += ExitRoom;
        model.StartGameView += StartGame;

        // hlcs.onValueChanged.AddListener(Hlcs);
        // tssj.onValueChanged.AddListener(Tssj);
        quickJoin.onClick.AddListener(ClickQuickJoin);
        general.onClick.AddListener(ClickGeneral);
        personal.onClick.AddListener(ClickPersonal);

        // Tssj(true);

        BGM.Instance.Load(Url.AUDIO + "bgm/outbgm_2.mp3");
        WebSocket.Instance.Connect();

        if (model.players != null && model.players.Count > 0)
        {
            JoinRoom();
        }
    }

    private void OnDestroy()
    {
        model.JoinRoomView -= JoinRoom;
        model.ExitRoomView -= ExitRoom;
        model.StartGameView -= StartGame;
    }

    // private void Hlcs(bool value)
    // {
    //     if (value)
    //     {
    //         mode = Mode.欢乐成双;
    //         modeBg.sprite = hlcsImg;
    //     }
    //     hlcs.GetComponent<Text>().fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
    // }

    // private void Tssj(bool value)
    // {
    //     Debug.Log(value);
    //     if (value)
    //     {
    //         mode = Mode.统帅双军;
    //         modeBg.sprite = tssjImg;
    //     }
    //     tssj.GetComponent<Text>().fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
    // }

    private void ClickQuickJoin()
    {
        GameCore.Room.Instance.JoinRoom(mode);
    }

    public void JoinRoom()
    {
        lobby.SetActive(false);
        room.SetActive(true);
    }

    public void ExitRoom()
    {
        room.SetActive(false);
        lobby.SetActive(true);
    }

    public void StartGame()
    {
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