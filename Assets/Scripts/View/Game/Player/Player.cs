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
    public Image nearDeath;
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
    public Image turnBorder;

    // 判定区
    public Transform judgeArea;
    public GameObject judgeCardPrefab;

    public GameCore.Player model { get; private set; }

    private void Start()
    {
        // 武将
        // Model.SgsMain.Instance.AfterBanPickView += InitGeneral;

        // 回合
        GameCore.TurnSystem.Instance.StartTurnView += StartTurn;
        GameCore.TurnSystem.Instance.FinishTurnView += FinishTurn;

        // 改变体力
        GameCore.UpdateHp.ActionView += UpdateHp;
        GameCore.UpdateHp.ActionView += NearDeath;

        // 阵亡
        GameCore.Die.ActionView += OnDead;

        // 判定区
        if (judgeArea.gameObject.activeSelf)
        {
            GameCore.DelayScheme.AddJudgeView += AddJudgeCard;
            GameCore.DelayScheme.RemoveJudgeView += RemoveJudgeCard;
        }

        // 横置
        GameCore.SetLock.ActionView += OnLock;
        // 翻面
        GameCore.TurnOver.ActionView += OnTurnOver;

        // 换肤
        if (this != GameMain.Instance.self) model.ChangeSkinView += general.UpdateSkin;
        else foreach (var i in model.teammates) i.ChangeSkinView += ChangeSkin;
        // GameCore.Player.ChangeSkinView += general.UpdateSkin;
    }

    private void OnDestroy()
    {
        // 武将
        // Model.SgsMain.Instance.AfterBanPickView -= InitGeneral;

        // 回合
        GameCore.TurnSystem.Instance.StartTurnView -= StartTurn;
        GameCore.TurnSystem.Instance.FinishTurnView -= FinishTurn;

        // 改变体力
        GameCore.UpdateHp.ActionView -= UpdateHp;
        GameCore.UpdateHp.ActionView -= NearDeath;

        // 阵亡
        GameCore.Die.ActionView -= OnDead;

        // 判定区
        if (judgeArea.gameObject.activeSelf)
        {
            GameCore.DelayScheme.AddJudgeView -= AddJudgeCard;
            GameCore.DelayScheme.RemoveJudgeView -= RemoveJudgeCard;
        }

        // 横置
        GameCore.SetLock.ActionView -= OnLock;
        GameCore.TurnOver.ActionView -= OnTurnOver;

        if (this != GameMain.Instance.self) model.ChangeSkinView -= general.UpdateSkin;
        else foreach (var i in model.teammates) i.ChangeSkinView -= ChangeSkin;
        // GameCore.Player.ChangeSkinView -= general.UpdateSkin;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(GameCore.Player player)
    {
        model = player;
        positionImage.sprite = positionSprite[model.turnOrder];

        // 2v2
        if (GameCore.Mode.Instance is GameCore.TwoVSTwo) team.sprite = model.isSelf ? teamSprites[0] : teamSprites[1];

        // 3v3
        else if (GameCore.Mode.Instance is GameCore.ThreeVSThree)
        {
            // 主将
            if (model.isMaster) team.sprite = model.team == GameCore.Team.BLUE ? teamSprites[2] : teamSprites[3];

            // 先锋
            else team.sprite = model.team == GameCore.Team.BLUE ? teamSprites[4] : teamSprites[5];
        }

        nearDeath.gameObject.SetActive(model.hp < 1 && model.hpLimit > 0);
        death.gameObject.SetActive(false);
        TurnOver.SetActive(player.isTurnOver);
        general.skin.material = null;

        general.Init(model);
    }

    private void ChangeSkin(GameCore.Skin skin)
    {
        if (skin.general_id == general.model.id) general.UpdateSkin(skin);
    }

    private void UpdateHp(GameCore.UpdateHp operation)
    {
        if (operation.player != model) return;
        general.SetHp(model.hp, model.hpLimit);
    }

    private void NearDeath(GameCore.UpdateHp operation)
    {
        if (operation.player != model) return;
        nearDeath.gameObject.SetActive(model.hp < 1);
    }

    private void OnDead(GameCore.Die operation)
    {
        if (operation.player != model) return;
        nearDeath.gameObject.SetActive(false);
        general.skin.material = gray;
        death.gameObject.SetActive(true);
        death.sprite = model.isSelf ? selfDead : oppoDead;
    }

    private void StartTurn()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;
        turnBorder.gameObject.SetActive(true);
    }

    private void FinishTurn()
    {
        if (GameCore.TurnSystem.Instance.CurrentPlayer != model) return;
        turnBorder.gameObject.SetActive(false);
    }

    private void AddJudgeCard(GameCore.DelayScheme card)
    {
        if (card.Owner != model) return;

        var instance = Instantiate(judgeCardPrefab);
        instance.transform.SetParent(judgeArea, false);
        instance.name = card.name;
        instance.GetComponent<Image>().sprite = Sprites.Instance.judgeCard[card.name];
    }

    private void RemoveJudgeCard(GameCore.DelayScheme card)
    {
        if (card.Owner != model) return;

        Destroy(judgeArea.Find(card.name)?.gameObject);
    }

    private void OnLock(GameCore.SetLock setLock)
    {
        if (setLock.player != model) return;
        Lock.SetActive(setLock.player.locked);
    }

    private void OnTurnOver(GameCore.TurnOver turnOver)
    {
        if (turnOver.player != model) return;
        TurnOver.SetActive(model.isTurnOver);
    }
}