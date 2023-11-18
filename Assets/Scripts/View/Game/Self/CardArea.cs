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
    public List<HandCard> handcards = new();

    private GameCore.Player self => GameMain.Instance.self.model;
    private GameCore.Timer timer => GameCore.Timer.Instance;

    // 已选卡牌
    private List<GameCore.Card> SelectedCards => GameCore.Timer.Instance.temp.cards;

    // 是否满足条件，即选择了规定数量的牌
    public bool IsValid { get; private set; } = false;

    private void Start()
    {
        // 手牌区
        GameCore.Timer.Instance.StopTimerView += Reset;

        // 获得牌
        GameCore.GetCard.ActionView += TeammateAdd;
        GameCore.GetCard.ActionView += UpdateHandCardText;

        // 失去牌
        GameCore.LoseCard.ActionView += Remove;
        GameCore.LoseCard.ActionView += UpdateHandCardText;

        // 改变体力
        GameCore.UpdateHp.ActionView += UpdateHandCardText;

        // 移动座位
        GameCore.Main.Instance.MoveSeatView += MoveSeat;
        GameCore.Main.Instance.MoveSeatView += UpdateHandCardText;
    }

    private void OnDestroy()
    {
        GameCore.Timer.Instance.StopTimerView -= Reset;

        GameCore.GetCard.ActionView -= TeammateAdd;
        GameCore.GetCard.ActionView -= UpdateHandCardText;

        GameCore.LoseCard.ActionView -= Remove;
        GameCore.LoseCard.ActionView -= UpdateHandCardText;

        GameCore.UpdateHp.ActionView -= UpdateHandCardText;

        GameCore.Main.Instance.MoveSeatView -= MoveSeat;
        GameCore.Main.Instance.MoveSeatView -= UpdateHandCardText;
    }

    /// <summary>
    /// 添加手牌
    /// </summary>
    public void Add(Card card)
    {
        if (handcards.Find(x => x.model == card.model) is HandCard handCard)
        {
            handcards.Remove(handCard);
            Destroy(handCard.gameObject);
        }

        handcards.Add(card.handCard);
        card.SetParent(handCardArea);
    }

    /// <summary>
    /// 添加队友手牌
    /// </summary>
    public void TeammateAdd(GameCore.GetCard operation)
    {
        if (!operation.player.isSelf || operation.player == self) return;

        foreach (var i in operation.Cards)
        {
            var card = Card.NewHandCard(i);
            card.gameObject.SetActive(false);
            Add(card);
        }
    }

    /// <summary>
    /// 移动座位
    /// </summary>
    public void MoveSeat(GameCore.Player model)
    {
        foreach (var i in handcards) i.gameObject.SetActive(model.handCards.Contains(i.model));
        MoveAll(0);
    }

    /// <summary>
    /// 移除手牌
    /// </summary>
    public void Remove(GameCore.LoseCard operation)
    {
        if (!operation.player.isSelf) return;

        foreach (var i in operation.Cards)
        {
            if (handcards.Find(x => x.model == i) is not HandCard handCard) continue;

            // 若卡牌还在队友手中，则不移除
            if (i.isHandCard && i.src.isSelf) handCard.gameObject.SetActive(self != operation.player);

            else
            {
                handcards.Remove(handCard);
                Destroy(handCard.gameObject);
            }
        }
        MoveAll(0.1f);
    }

    /// <summary>
    /// 初始化手牌区
    /// </summary>
    public void OnStartPlay()
    {
        // 无懈可击
        if (timer.type == GameCore.Timer.Type.WXKJ)
        {
            foreach (var i in handcards) i.gameObject.SetActive(i.model.Useable<GameCore.无懈可击>());
            MoveAll(0);
        }

        // 仁德 眩惑
        if (timer.multiConvert.Count > 0) AddConvertedCard();

        // 对不能使用的牌设置阴影
        foreach (var i in handcards.Where(x => x.gameObject.activeSelf)) i.SetDisabledColor();

        Update_();
    }

    /// <summary>
    /// 重置手牌区（进度条结束时调用）
    /// </summary>
    public void Reset()
    {
        if (!timer.players.Contains(self)) return;

        // 重置手牌状态
        foreach (var card in handcards.Where(x => x.gameObject.activeSelf)) card.Reset();

        if (timer.type == GameCore.Timer.Type.WXKJ)
        {
            foreach (var i in handcards) i.gameObject.SetActive(self.handCards.Contains(i.model));
            MoveAll(0);
        }

        if (timer.multiConvert.Count > 0)
        {
            foreach (var i in handcards.Where(x => x.model.isConvert).ToList())
            {
                handcards.Remove(i);
                Destroy(i.gameObject);
            }
            MoveAll(0.1f);
        }

        IsValid = false;
    }

    /// <summary>
    /// 更新手牌区
    /// </summary>
    public void Update_()
    {
        // 若已选中手牌数量超出范围，取消第一张选中的手牌
        while (SelectedCards.Count > timer.maxCard)
        {
            handcards.Find(x => x.model == SelectedCards[0])?.Unselect();
            EquipArea.Instance.Equips.Values.FirstOrDefault(x => x.model == SelectedCards[0])?.Unselect();
        }

        // 判断是否已符合要求
        IsValid = SelectedCards.Count >= timer.minCard;

        // 判断每张卡牌是否可选
        if (timer.maxCard == 0) return;
        foreach (var i in handcards)
        {
            if (!i.gameObject.activeSelf) continue;
            i.toggle.interactable = timer.isValidCard(i.model);
        }
    }

    private void AddConvertedCard()
    {
        foreach (var i in timer.multiConvert)
        {
            var card = Card.NewHandCard(i);
            card.SetParent(handCardArea);
            handcards.Add(card.handCard);
        }
        MoveAll(0);
    }

    public GridLayoutGroup gridLayoutGroup;

    /// <summary>
    /// 更新卡牌间距
    /// </summary>
    private void UpdateSpacing()
    {
        int count = handcards.Where(x => x.gameObject.activeSelf).Count();
        float spacing = count >= 8 ? -(count * 121.5f - 950) / (float)(count - 1) - 0.001f : 0;
        gridLayoutGroup.spacing = new Vector2(spacing, 0);
    }

    public async void MoveAll(float second)
    {
        UpdateSpacing();
        await Util.WaitFrame();
        foreach (var i in handcards) i.card.Move(second);
    }


    /// <summary>
    /// 更新手牌数与手牌上限信息
    /// </summary>
    private void UpdateHandCardText()
    {
        handCardText.text = self.handCardsCount.ToString() + "/" + self.handCardsLimit.ToString();
    }

    private void UpdateHandCardText(GameCore.Player player)
    {
        if (self != player) return;
        UpdateHandCardText();
    }

    private void UpdateHandCardText(GameCore.GetCard operation)
    {
        if (self != operation.player) return;
        UpdateHandCardText();
    }

    private void UpdateHandCardText(GameCore.LoseCard operation)
    {
        if (self != operation.player) return;
        UpdateHandCardText();
    }

    private void UpdateHandCardText(GameCore.UpdateHp operation)
    {
        if (self != operation.player) return;
        UpdateHandCardText();
    }
}