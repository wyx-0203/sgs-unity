using Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardArea : SingletonMono<CardArea>
{
    // 手牌区
    public Transform handCardArea;
    // 手牌数
    public Text handCardText;
    // 手牌
    public List<HandCard> handCards = new();

    private Model.Player self => Game.Instance.firstPerson.model;
    private SinglePlayQuery current => PlayArea.Instance.current;

    private PlayDecision decision => PlayArea.Instance.decision;

    // 是否满足条件，即选择了规定数量的牌
    public bool IsValid { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();

        // 手牌区
        EventSystem.Instance.AddEvent<FinishPlay>(OnReset);

        // 获得/失去牌
        EventSystem.Instance.AddEvent<GetCard>(TeammateAdd);
        EventSystem.Instance.AddEvent<LoseCard>(Remove);
        EventSystem.Instance.AddEvent<UpdateCard>(UpdateHandCardText);

        // 改变体力
        EventSystem.Instance.AddEvent<UpdateHp>(UpdateHandCardText);

        // 切换视角
        Game.Instance.OnChangeView += OnChangeView;
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<FinishPlay>(OnReset);

        EventSystem.Instance.RemoveEvent<GetCard>(TeammateAdd);
        EventSystem.Instance.RemoveEvent<LoseCard>(Remove);
        EventSystem.Instance.RemoveEvent<UpdateCard>(UpdateHandCardText);

        EventSystem.Instance.RemoveEvent<UpdateHp>(UpdateHandCardText);
    }

    /// <summary>
    /// 添加手牌
    /// </summary>
    public void Add(Card card)
    {
        if (handCards.Find(x => x.model == card.model) is HandCard handCard)
        {
            handCards.Remove(handCard);
            Destroy(handCard.gameObject);
            Debug.Log("destroy");
        }

        handCards.Add(card.GetComponent<HandCard>());
        card.SetParent(handCardArea);
    }

    /// <summary>
    /// 添加队友手牌
    /// </summary>
    public void TeammateAdd(GetCard getCard)
    {
        // Debug.Log(getCard.player);
        // Debug.Log(self.index);
        var player = Player.Find(getCard.player).model;
        if (!player.isSelf || player == self) return;

        foreach (var i in getCard.cards)
        {
            var card = Card.NewHandCard(i);
            card.gameObject.SetActive(false);
            Add(card);
        }
    }

    /// <summary>
    /// 移动座位
    /// </summary>
    public void OnChangeView(Model.Player player)
    {
        foreach (var i in handCards) i.gameObject.SetActive(player.handCards.Contains(i.model));
        MoveAll(0);
        UpdateHandCardText(player.handCards.Count, player.handCardsLimit);
    }

    /// <summary>
    /// 移除手牌
    /// </summary>
    public void Remove(LoseCard loseCard)
    {
        if (!Player.IsSelf(loseCard.player)) return;

        foreach (var i in loseCard.cards)
        {
            if (handCards.Find(x => x.id == i) is not HandCard handCard) continue;

            handCards.Remove(handCard);
            Destroy(handCard.gameObject);
        }
        MoveAll(0.1f);
    }

    /// <summary>
    /// 初始化手牌区
    /// </summary>
    public void OnStartPlay()
    {
        // 无懈可击
        if (current.type == SinglePlayQuery.Type.WXKJ)
        {
            foreach (var i in handCards)
            {
                var t = i.name == "无懈可击";
                i.gameObject.SetActive(t);
                i.toggle.interactable = t;
            }
            MoveAll(0);
            return;
        }

        // 判断每张卡牌是否可选
        foreach (var i in handCards.Where(x => x.gameObject.activeSelf))
        {
            i.toggle.interactable = current.cards.Contains(i.id);
            // 回合内不可选的手牌变暗
            i.SetDisabledColor();
        }

        IsValid = current.minCard == 0;
    }

    /// <summary>
    /// 重置手牌区（进度条结束时调用）
    /// </summary>
    public void Reset()
    {
        IsValid = false;

        // 重置手牌状态
        inReset = true;
        foreach (var card in handCards.Where(x => x.gameObject.activeSelf)) card.Reset();
        inReset = false;

        if (current.type == SinglePlayQuery.Type.WXKJ)
        {
            foreach (var i in handCards) i.gameObject.SetActive(self.handCards.Contains(i.model));
            MoveAll(0);
        }
    }

    private void OnReset(FinishPlay finishPlay)
    {
        if (Player.IsSelf(finishPlay.player)) Reset();
    }

    private bool inReset;

    /// <summary>
    /// 更新手牌区，选中卡牌时触发
    /// </summary>
    public void OnClickCard(int cardId, bool value)
    {
        if (value) decision.cards.Add(cardId);
        else decision.cards.Remove(cardId);
        // if (!value) 
        if (inReset) return;

        // 若已选中手牌数量超出范围，取消第一张选中的手牌
        if (decision.cards.Count > current.maxCard)
        {
            var card = handCards.Find(x => x.id == decision.cards[0]);
            if (card != null) card.toggle.isOn = false;
            else EquipArea.Instance.equipments.Values.First(x => x.id == decision.cards[0]).toggle.isOn = false;
            return;
        }

        // 判断是否已符合要求
        IsValid = decision.cards.Count >= current.minCard;

        // 判断每张卡牌是否可选
        // 乱击
        if (current.type == SinglePlayQuery.Type.LuanJi)
        {
            foreach (var i in handCards.Where(x => x.gameObject.activeSelf))
            {
                i.toggle.interactable = current.cards.Contains(i.id)
                    && (decision.cards.Count == 0 || i.model.suit == Model.Card.Find(decision.cards[0]).suit);
            }
        }

        VirtualCardArea.Instance.Reset();
        DestArea.Instance.Reset();
        VirtualCardArea.Instance.OnStartPlay();
        DestArea.Instance.OnStartPlay();
        PlayArea.Instance.UpdateButtonArea();
    }

    public GridLayoutGroup gridLayoutGroup;

    /// <summary>
    /// 更新卡牌间距
    /// </summary>
    private void UpdateSpacing()
    {
        int count = handCards.Where(x => x.gameObject.activeSelf).Count();
        float spacing = count >= 8 ? -(count * 121.5f - 950) / (float)(count - 1) - 0.001f : 0;
        gridLayoutGroup.spacing = new Vector2(spacing, 0);
    }

    public async void MoveAll(float second)
    {
        UpdateSpacing();
        await Util.WaitFrame();
        foreach (var i in handCards) i.card.Move(second);
    }


    /// <summary>
    /// 更新手牌数与手牌上限信息
    /// </summary>
    private void UpdateHandCardText(int count, int limit)
    {
        handCardText.text = $"{count}/{limit}";
    }

    private void UpdateHandCardText(Model.UpdateCard updateCard)
    {
        if (updateCard.player == self.index) UpdateHandCardText(self.handCards.Count, self.handCardsLimit);
    }

    private void UpdateHandCardText(Model.UpdateHp updateHp)
    {
        if (updateHp.player == self.index) UpdateHandCardText(self.handCards.Count, updateHp.handCardsLimit);
    }
}
