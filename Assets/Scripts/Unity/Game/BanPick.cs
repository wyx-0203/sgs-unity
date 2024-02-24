using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BanPick : SingletonMono<BanPick>
{
    public List<GeneralBP> generals { get; } = new();
    // 倒计时读条
    public Slider timer;
    // 提示
    public Text hint;

    // private GameCore.BanPick model => GameCore.BanPick.Instance;
    // private GameCore.Team selfTeam => GameCore.Self.Instance.team;

    // public GameObject generalPrefab;
    public Transform pool;
    public Transform selfPool;

    public async void Show(List<int> _pool)
    {
        gameObject.SetActive(true);
        // model.StartPickView += StartPick;
        // model.OnPickView += OnPick;
        // model.StartBanView += StartBan;
        // model.OnBanView += OnBan;
        // model.StartSelfPickView += SelfPick;
        EventSystem.Instance.AddEvent<PickQuery>(StartPick);
        EventSystem.Instance.AddEvent<OnPick>(OnPick);
        EventSystem.Instance.AddEvent<BanQuery>(StartBan);
        EventSystem.Instance.AddEvent<OnBan>(OnBan);
        EventSystem.Instance.AddEvent<StartSelfPick>(OnStartSelfPick);

        foreach (var i in _pool)
        {
            var general = Instantiate(GameAsset.Instance.general, pool);
            general.Init(i);
            generals.Add(general);
        }

        contentSizeFitter = pool.GetComponent<ContentSizeFitter>();
        gridLayoutGroup = pool.GetComponent<GridLayoutGroup>();

        await Util.WaitFrame();

        contentSizeFitter.enabled = false;
        gridLayoutGroup.enabled = false;
    }

    private void OnDestroy()
    {
        // model.StartPickView -= StartPick;
        // model.OnPickView -= OnPick;
        // model.StartBanView -= StartBan;
        // model.OnBanView -= OnBan;
        // model.StartSelfPickView -= SelfPick;
        EventSystem.Instance.RemoveEvent<PickQuery>(StartPick);
        EventSystem.Instance.RemoveEvent<OnPick>(OnPick);
        EventSystem.Instance.RemoveEvent<BanQuery>(StartBan);
        EventSystem.Instance.RemoveEvent<OnBan>(OnBan);
        EventSystem.Instance.RemoveEvent<StartSelfPick>(OnStartSelfPick);
    }

    private void StartPick(PickQuery pickQuery)
    {
        StartCoroutine(StartTimer(pickQuery.second));

        if (Player.IsSelf(pickQuery.player))
        {
            foreach (var i in generals.Where(x => x.state == GeneralBP.State.Selectable)) i.button.interactable = true;
            hint.text = "请点击选择武将";
        }
        else hint.text = "等待对方选将";
    }

    private void OnPick(OnPick onPick)
    {
        Reset();
        generals.Find(x => x.id == onPick.general)?.OnPick(Player.IsSelf(onPick.player));
    }

    private void StartBan(BanQuery banQuery)
    {
        StartCoroutine(StartTimer(banQuery.second));

        if (Player.IsSelf(banQuery.player))
        {
            foreach (var i in generals.Where(x => x.state == GeneralBP.State.Selectable)) i.button.interactable = true;
            hint.text = "请点击禁用武将";
        }
        else hint.text = "等待对方禁将";
    }

    private void OnBan(OnBan onBan)
    {
        Reset();
        generals.Find(x => x.id == onBan.general)?.OnBan();
    }

    public void OnClickGeneral(int id)
    {
        EventSystem.Instance.SendToServer(new GeneralDecision
        {
            player = Game.Instance.firstPerson.model.index,
            general = id
        });
    }


    private GridLayoutGroup gridLayoutGroup;
    private ContentSizeFitter contentSizeFitter;

    public Button commit;
    public Transform seatParent;
    // public GameObject seatPrefab;
    public List<SelfPickSeat> seats { get; } = new();


    private async void OnStartSelfPick(StartSelfPick ssp)
    {
        hint.text = "请选择己方要出场的武将";
        commit.onClick.AddListener(OnSubmit);

        foreach (var i in generals)
        {
            // 销毁被禁的武将
            if (i.state == GeneralBP.State.Ban) Destroy(i.gameObject);
            // 设置己方武将
            else if (i.state == GeneralBP.State.Self) i.ToSelfPick();
        }
        Destroy(selfPool.gameObject);

        // 设置屏幕底部的座位
        foreach (var i in Game.Instance.players.Where(x => x.model.isSelf))
        {
            var seat = Instantiate(GameAsset.Instance.seat, seatParent);
            seat.Init(i.model);
            seats.Add(seat);
        }
        // seats = seatParent.Cast<Transform>().Select(x => x.GetComponent<SelfPickSeat>()).ToList();

        StartCoroutine(StartTimer(ssp.second));

        gridLayoutGroup.enabled = true;
        contentSizeFitter.enabled = true;

        await Util.WaitFrame();

        contentSizeFitter.enabled = false;
        gridLayoutGroup.enabled = false;
    }

    private IEnumerator StartTimer(int second)
    {
        timer.value = 1;
        while (timer.value > 0)
        {
            timer.value -= Time.deltaTime / second;
            yield return null;
        }
    }

    private void Reset()
    {
        StopAllCoroutines();
        foreach (var i in generals) i.button.interactable = false;
    }

    public void UpdateCommitButton()
    {
        commit.gameObject.SetActive(seats.FirstOrDefault(x => x.general is null) is null);
    }

    private async void OnSubmit()
    {
        await System.Threading.Tasks.Task.Yield();
        foreach (var i in seats)
        {
            EventSystem.Instance.SendToServer(new GeneralDecision
            {
                player = i.player.index,
                general = i.general.id
            });
        }
        // GameCore.BanPick.Instance.SendSelfResult(selfTeam, seats.Select(x => x.general.model.id).ToList());
        commit.gameObject.SetActive(false);
    }
}