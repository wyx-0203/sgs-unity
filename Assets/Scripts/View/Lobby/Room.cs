using UnityEngine.UI;

public class Room : SingletonMono<Room>
{
    GameCore.Room model => GameCore.Room.Instance;
    public Seat[] allSeats;
    private Seat[] seats;
    public Toggle already;
    public Text alreadyText;
    public Button start;
    public Button back;

    private void Start()
    {
        model.AddPlayerView += AddPlayer;
        model.RemovePlayerView += RemovePlayer;
        model.SetAlreadyView += SetAlready;

        already.onValueChanged.AddListener(ClickAlready);
        start.onClick.AddListener(ClickStart);
        back.onClick.AddListener(ClickBack);
    }

    private void OnDestroy()
    {
        model.AddPlayerView -= AddPlayer;
        model.RemovePlayerView -= RemovePlayer;
        model.SetAlreadyView -= SetAlready;
    }

    private void OnEnable()
    {
        // if (model.mode is Mode.统帅双军) 
        seats = allSeats[0..2];
        // else seats = allSeats[2..6];

        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].gameObject.SetActive(true);
            seats[i].AddPlayer(model.Users[i]);
        }

        start.gameObject.SetActive(model.self.owner);
        already.gameObject.SetActive(!model.self.owner);
        alreadyText.text = "准备";
    }

    private void OnDisable()
    {
        foreach (var i in seats) i.gameObject.SetActive(false);
    }

    private void ClickAlready(bool value)
    {
        // alreadyText.text = value ? "取消" : "准备";
        model.SendSetAlready();
    }

    private void ClickStart()
    {
        model.SendStartGame();
    }

    private void ClickBack()
    {
        model.ExitRoom();
    }

    /// <summary>
    /// 玩家进入
    /// </summary>
    public void AddPlayer(UserJson user)
    {
        seats[user.position].AddPlayer(user);
    }

    /// <summary>
    /// 玩家退出
    /// </summary>
    public void RemovePlayer(int position, int ownerPos)
    {
        seats[position].RemovePlayer();

        seats[ownerPos].UpdateStatus();
        if (model.self.owner)
        {
            start.gameObject.SetActive(true);
            already.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 准备或取消准备
    /// </summary>
    public void SetAlready(UserJson user, bool already)
    {
        seats[user.position].already.SetActive(already);

        // 若为自己准备，改变准备按钮
        if (user == model.self)
        {
            alreadyText.text = already ? "取消" : "准备";
        }

        // 若自己为房主，设置开始按钮
        else if (model.self.owner)
        {
            foreach (var i in seats)
            {
                if (!i.already.activeSelf && !i.owner.activeSelf)
                {
                    start.interactable = false;
                    return;
                }
            }
            start.interactable = true;
        }
    }
}