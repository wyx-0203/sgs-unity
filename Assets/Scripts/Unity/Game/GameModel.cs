using Model;
using System.Linq;

public class GameModel : SingletonMono<GameModel>
{
    protected override void Awake()
    {
        base.Awake();

        EventSystem.Instance.AddPriorEvent<InitPlayer>(OnInitPlayer);
        EventSystem.Instance.AddPriorEvent<InitGeneral>(OnInitGeneral);

        // 卡牌
        EventSystem.Instance.AddPriorEvent<GetCard>(OnGetCard);
        EventSystem.Instance.AddPriorEvent<LoseCard>(OnLoseCard);
        EventSystem.Instance.AddPriorEvent<AddEquipment>(OnAddEquipment);

        // 改变体力
        EventSystem.Instance.AddPriorEvent<UpdateHp>(UpdateHp);

        // 阵亡
        EventSystem.Instance.AddPriorEvent<Die>(OnDead);

        // 判定区
        EventSystem.Instance.AddPriorEvent<AddJudgeCard>(OnAddJudgeCard);
        EventSystem.Instance.AddPriorEvent<RemoveJudgeCard>(OnRemoveJudgeCard);

        // 横置
        EventSystem.Instance.AddPriorEvent<SetLock>(OnLock);
        // 翻面
        EventSystem.Instance.AddPriorEvent<TurnOver>(OnTurnOver);
        // 获得/失去技能
        EventSystem.Instance.AddPriorEvent<UpdateSkill>(OnUpdateSkill);

        // 换肤
        EventSystem.Instance.AddPriorEvent<ChangeSkin>(OnChangeSkin);
    }

    public Model.Player[] players;
    public Team selfTeam;

    private void OnInitPlayer(InitPlayer initPlayer)
    {
        selfTeam = initPlayer.GetTeam(Global.Instance.userId);

        int n = initPlayer.team.Count;
        players = new Model.Player[n];
        for (int i = 0; i < n; i++)
        {
            players[i] = new Model.Player
            {
                index = i,
                team = initPlayer.team[i],
                isSelf = initPlayer.team[i] == selfTeam,
                turnOrder = initPlayer.position[i],
                isMonarch = initPlayer.isMonarch[i]
            };
        }
    }

    private void OnInitGeneral(InitGeneral initGeneral)
    {
        var player = players[initGeneral.player];

        player.hp = initGeneral.hp;
        player.hpLimit = initGeneral.hpLimit;
        player.skills = initGeneral.skills;
        player.general = Model.General.Get(initGeneral.general);
        player.currentSkin = player.general.skins[0];
    }

    private void OnChangeSkin(ChangeSkin changeSkin)
    {
        players[changeSkin.player].currentSkin = changeSkin.skinId;
    }

    private void UpdateHp(UpdateHp updateHp)
    {
        players[updateHp.player].hp = updateHp.hp;
    }

    private void OnDead(Die die)
    {
        players[die.player].alive = false;
    }

    private void OnAddJudgeCard(AddJudgeCard ajc)
    {
        players[ajc.player].JudgeCards.Add(Model.Card.Find(ajc.card));
    }

    private void OnRemoveJudgeCard(RemoveJudgeCard rjc)
    {
        players[rjc.player].JudgeCards.Remove(Model.Card.Find(rjc.card));
    }

    private void OnLock(SetLock setLock)
    {
        players[setLock.player].locked = setLock.value;
    }

    private void OnTurnOver(TurnOver turnOver)
    {
        players[turnOver.player].isTurnOver = turnOver.value;
    }

    private void OnGetCard(GetCard getCard)
    {
        players[getCard.player].handCards.AddRange(getCard.cards.Select(x => Model.Card.Find(x)));
    }

    private void OnLoseCard(LoseCard loseCard)
    {
        var player = players[loseCard.player];
        foreach (var i in loseCard.cards)
        {
            var card = Model.Card.Find(i);
            if (player.handCards.Contains(card)) player.handCards.Remove(card);
            else player.Equipments.Remove(card.type);
        }
    }

    private void OnAddEquipment(AddEquipment addEquipment)
    {
        var card = Model.Card.Find(addEquipment.card);
        players[addEquipment.player].Equipments.Add(card.type, card);
    }

    private void OnUpdateSkill(UpdateSkill updateSkill)
    {
        players[updateSkill.player].skills = updateSkill.skills;
    }
}
