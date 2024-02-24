using System.Linq;
using UnityEngine.UI;

public class Room : SingletonMono<Room>
{
    public Seat[] allSeats;
    private Seat[] seats;
    public Toggle already;
    public Text alreadyText;
    public Button start;
    public Button back;

    public Seat self { get; private set; }
    private int ownerId;
    public bool IsOwner(int userId) => userId == ownerId;
    public bool IsSelf(int userId) => userId == Global.Instance.userId;
    private Seat GetSeat(int userId) => seats.First(x => x.userId == userId);

    private void Start()
    {
        already.onValueChanged.AddListener(ClickAlready);
        start.onClick.AddListener(ClickStart);
        back.onClick.AddListener(ClickBack);

        EventSystem.Instance.AddEvent<Model.AddUser>(OnAddUser);
        EventSystem.Instance.AddEvent<Model.RemoveUser>(OnRemoveUser);
        EventSystem.Instance.AddEvent<Model.SetAlready>(OnSetAlready);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<Model.AddUser>(OnAddUser);
        EventSystem.Instance.RemoveEvent<Model.RemoveUser>(OnRemoveUser);
        EventSystem.Instance.RemoveEvent<Model.SetAlready>(OnSetAlready);
    }

    public void Show(Model.JoinRoom joinRoom)
    {
        gameObject.SetActive(true);
        // if (model.mode is Mode.统帅双军) 
        seats = allSeats[0..2];
        // else seats = allSeats[2..6];
        ownerId = joinRoom.ownerId;

        for (int i = 0; i < seats.Length; i++) seats[i].gameObject.SetActive(true);
        foreach (var i in joinRoom.users) seats[i.position].AddUser(i);

        self = seats.First(x => IsSelf(x.userId));

        start.gameObject.SetActive(IsOwner(self.userId));
        already.gameObject.SetActive(!IsOwner(self.userId));
        alreadyText.text = "准备";
    }

    private void ClickAlready(bool value)
    {
        Connection.Instance.SetAlready(value);
    }

    private void ClickStart()
    {
        Connection.Instance.StartGame();
    }

    private void ClickBack()
    {
        Connection.Instance.ExitRoom();
    }

    /// <summary>
    /// 玩家进入
    /// </summary>
    public void OnAddUser(Model.AddUser addUser)
    {
        seats[addUser.user.position].AddUser(addUser.user);
    }

    /// <summary>
    /// 玩家退出
    /// </summary>
    private void OnRemoveUser(Model.RemoveUser removeUser)
    {
        // 自己退出
        if (removeUser.userId == Global.Instance.userId)
        {
            foreach (var i in seats) i.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        // 其他人退出
        else
        {
            GetSeat(removeUser.userId).RemovePlayer();
            if (ownerId == removeUser.ownerId) return;

            // 设置新房主
            ownerId = removeUser.ownerId;
            GetSeat(ownerId).owner.SetActive(true);

            // 自己为房主
            if (ownerId == Global.Instance.userId)
            {
                start.gameObject.SetActive(true);
                already.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 准备或取消准备
    /// </summary>
    private void OnSetAlready(Model.SetAlready setAlready)
    {
        GetSeat(setAlready.userId).already.SetActive(setAlready.value);

        // 若为自己准备，改变准备按钮
        if (IsSelf(setAlready.userId))
        {
            alreadyText.text = already.isOn ? "取消" : "准备";
        }

        // 若自己为房主，设置开始按钮
        else if (IsSelf(ownerId))
        {
            start.interactable = seats.All(x => x.already.activeSelf || IsOwner(x.userId));
        }
    }
}