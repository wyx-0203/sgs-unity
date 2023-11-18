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

    private GameCore.BanPick model => GameCore.BanPick.Instance;
    private GameCore.Team selfTeam => GameCore.Self.Instance.team;

    public GameObject generalPrefab;
    public Transform pool;
    public Transform selfPool;

    private async void Start()
    {
        model.StartPickView += StartPick;
        model.OnPickView += OnPick;
        model.StartBanView += StartBan;
        model.OnBanView += OnBan;
        model.StartSelfPickView += SelfPick;

        foreach (var i in model.Pool)
        {
            var general = Instantiate(generalPrefab, pool).GetComponent<GeneralBP>();
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
        model.StartPickView -= StartPick;
        model.OnPickView -= OnPick;
        model.StartBanView -= StartBan;
        model.OnBanView -= OnBan;
        model.StartSelfPickView -= SelfPick;
    }

    private void StartPick()
    {
        StartCoroutine(StartTimer(model.second));

        if (model.Current == selfTeam)
        {
            foreach (var i in generals.Where(x => x.state == GeneralBP.State.Selectable)) i.button.interactable = true;
            hint.text = "请点击选择武将";
        }
        else hint.text = "等待对方选将";
    }

    private void OnPick(GameCore.General general)
    {
        Reset();
        generals.Find(x => x.model == general)?.OnPick(model.Current == selfTeam);
    }

    private void StartBan()
    {
        StartCoroutine(StartTimer(model.second));

        if (model.Current == selfTeam)
        {
            foreach (var i in generals.Where(x => x.state == GeneralBP.State.Selectable)) i.button.interactable = true;
            hint.text = "请点击禁用武将";
        }
        else hint.text = "等待对方禁将";
    }

    private void OnBan(GameCore.General general)
    {
        Reset();
        generals.Find(x => x.model == general)?.OnBan();
    }


    private GridLayoutGroup gridLayoutGroup;
    private ContentSizeFitter contentSizeFitter;

    public Button commit;
    public Transform seatParent;
    public GameObject seatPrefab;
    public List<SelfPickSeat> seats { get; private set; }


    private async void SelfPick()
    {
        hint.text = "请选择己方要出场的武将";
        commit.onClick.AddListener(ClickCommit);

        foreach (var i in BanPick.Instance.generals)
        {
            // 销毁被禁的武将
            if (i.state == GeneralBP.State.Ban) Destroy(i.gameObject);
            // 设置己方武将
            else if (i.state == GeneralBP.State.Self) i.ToSelfPick();
        }
        Destroy(selfPool.gameObject);

        // 设置屏幕底部的座位
        foreach (var i in selfTeam.GetAllPlayers()) Instantiate(seatPrefab, seatParent).GetComponent<SelfPickSeat>().Init(i);
        seats = seatParent.Cast<Transform>().Select(x => x.GetComponent<SelfPickSeat>()).ToList();

        StartCoroutine(StartTimer(model.second));

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

    private async void ClickCommit()
    {
        await System.Threading.Tasks.Task.Yield();
        GameCore.BanPick.Instance.SendSelfResult(selfTeam, seats.Select(x => x.general.model.id).ToList());
        commit.gameObject.SetActive(false);
    }
}