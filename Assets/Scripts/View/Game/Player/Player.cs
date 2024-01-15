using Model;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // 位置
    public Image positionImage;
    public Sprite[] positionSprite;

    public General general;

    // 阵营
    public Image team;
    public Sprite[] teamSprites;

    // 濒死状态
    public GameObject nearDeath;
    // 阵亡
    public Image death;
    public Sprite selfDead;
    public Sprite oppoDead;
    public Material gray;

    // 横置
    public GameObject Lock;
    // 翻面
    public GameObject TurnOver;

    // 回合内边框
    public GameObject turnBorder;

    // 判定区
    public Transform judgeArea;

    public Model.Player model { get; private set; }

    public static Player Find(int position) => GameMain.Instance.players[position];
    public static bool IsSelf(int position) => Find(position).model.isSelf;

    private void Awake()
    {
        // 武将
        EventSystem.Instance.AddEvent<InitGeneral>(OnInitGeneral);

        // 回合
        EventSystem.Instance.AddEvent<StartTurn>(OnStartTurn);
        EventSystem.Instance.AddEvent<FinishTurn>(OnFinishTurn);

        // 改变体力
        EventSystem.Instance.AddEvent<UpdateHp>(UpdateHp);
        EventSystem.Instance.AddEvent<UpdateHp>(NearDeath);

        // 阵亡
        EventSystem.Instance.AddEvent<Die>(OnDead);

        // 判定区
        EventSystem.Instance.AddEvent<AddJudgeCard>(OnAddJudgeCard);
        EventSystem.Instance.AddEvent<RemoveJudgeCard>(OnRemoveJudgeCard);

        // 横置
        EventSystem.Instance.AddEvent<SetLock>(OnLock);
        // 翻面
        EventSystem.Instance.AddEvent<TurnOver>(OnTurnOver);

        // 换肤
        EventSystem.Instance.AddEvent<ChangeSkin>(OnChangeSkin);
    }

    private void OnDestroy()
    {
        // 武将
        EventSystem.Instance.RemoveEvent<InitGeneral>(OnInitGeneral);

        // 回合
        EventSystem.Instance.RemoveEvent<StartTurn>(OnStartTurn);
        EventSystem.Instance.RemoveEvent<FinishTurn>(OnFinishTurn);

        // 改变体力
        EventSystem.Instance.RemoveEvent<UpdateHp>(UpdateHp);
        EventSystem.Instance.RemoveEvent<UpdateHp>(NearDeath);

        // 阵亡
        EventSystem.Instance.RemoveEvent<Die>(OnDead);

        // 判定区
        EventSystem.Instance.RemoveEvent<AddJudgeCard>(OnAddJudgeCard);
        EventSystem.Instance.RemoveEvent<RemoveJudgeCard>(OnRemoveJudgeCard);

        EventSystem.Instance.RemoveEvent<SetLock>(OnLock);
        EventSystem.Instance.RemoveEvent<TurnOver>(OnTurnOver);

        EventSystem.Instance.RemoveEvent<ChangeSkin>(OnChangeSkin);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(Model.Player player)
    {
        model = player;
        positionImage.sprite = positionSprite[model.turnOrder];

        // 2v2
        // if (GameCore.Mode.Instance is GameCore.TwoVSTwo) team.sprite = model.isSelf ? teamSprites[0] : teamSprites[1];

        // 3v3
        // else if (GameCore.Mode.Instance is GameCore.ThreeVSThree)
        // {
        // 主将
        if (model.isMonarch) team.sprite = model.team == Team.BLUE ? teamSprites[2] : teamSprites[3];

        // 先锋
        else team.sprite = model.team == Team.BLUE ? teamSprites[4] : teamSprites[5];

        nearDeath.SetActive(model.hp < 1 && model.hpLimit > 0);
        death.gameObject.SetActive(!model.alive);
        TurnOver.SetActive(player.isTurnOver);

    }

    public void OnInitGeneral(InitGeneral initGeneral)
    {
        if (initGeneral.player == model.index) general.Init(model);
    }

    private void OnChangeSkin(ChangeSkin changeSkin)
    {
        if (changeSkin.player == model.index)
        {
            general.skin.Set(Model.Skin.Get(changeSkin.skinId));
        }
    }

    private void UpdateHp(UpdateHp updateHp)
    {
        if (updateHp.player == model.index) general.SetHp(model.hp, model.hpLimit);
    }

    private void NearDeath(UpdateHp updateHp)
    {
        if (updateHp.player == model.index) nearDeath.SetActive(updateHp.hp < 1);
    }

    private void OnDead(Die die)
    {
        if (die.player != model.index) return;

        nearDeath.SetActive(false);
        general.skin.OnDead();
        death.gameObject.SetActive(true);
        death.sprite = model.isSelf ? selfDead : oppoDead;
    }

    private void OnStartTurn(StartTurn startTurn)
    {
        if (startTurn.player == model.index) turnBorder.SetActive(true);
    }

    private void OnFinishTurn(FinishTurn finishTurn)
    {
        if (finishTurn.player == model.index) turnBorder.SetActive(false);
    }

    private void OnAddJudgeCard(AddJudgeCard addJudgeCard)
    {
        if (addJudgeCard.player != model.index) return;

        // var instance = Instantiate(judgeCardPrefab);
        string name = Model.Card.Find(addJudgeCard.card).name;
        var instance = new GameObject(name).AddComponent<Image>();
        instance.transform.SetParent(judgeArea, false);
        // instance.name = name;
        instance.sprite = GameAssets.Instance.judgeCardImage.Get(name);
        instance.SetNativeSize();
    }

    private void OnRemoveJudgeCard(RemoveJudgeCard removeJudgeCard)
    {
        if (removeJudgeCard.player == model.index)
        {
            Destroy(judgeArea.Find(Model.Card.Find(removeJudgeCard.card).name).gameObject);
        }
    }

    private void OnLock(SetLock setLock)
    {
        if (setLock.player == model.index) Lock.SetActive(setLock.value);
    }

    private void OnTurnOver(TurnOver turnOver)
    {
        if (turnOver.player == model.index) TurnOver.SetActive(turnOver.value);
    }
}