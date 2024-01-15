using Model;
using System.Collections.Generic;
using System.Linq;

public class VirtualCardArea : SingletonMono<VirtualCardArea>
{
    public bool IsValid { get; private set; } = false;
    private SinglePlayQuery playQuery => PlayArea.Instance.current;
    private PlayDecision decision => PlayArea.Instance.decision;
    private List<HandCard> handCards => CardArea.Instance.handCards;

    private void Start()
    {
        EventSystem.Instance.AddEvent<FinishPlay>(Reset);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<FinishPlay>(Reset);
    }

    public void OnStartPlay()
    {
        if (!CardArea.Instance.IsValid) return;
        // UnityEngine.Debug.Log(1);

        if (playQuery.virtualCards.Count == 0)
        {
            IsValid = true;
            return;
        }

        foreach (var i in handCards.Where(x => x.gameObject.activeSelf)) i.toggle.interactable = false;

        foreach (var i in playQuery.virtualCards.Union(playQuery.disabledVirtualCards))
        {
            var card = Card.NewHandCard(i);
            card.SetParent(CardArea.Instance.handCardArea);
            var handCard = card.GetComponent<HandCard>();
            handCards.Add(handCard);
            handCard.SetDisabledColor();
            handCard.toggle.interactable = !playQuery.disabledVirtualCards.Contains(i);
        }

        CardArea.Instance.MoveAll(0);
    }

    // private HandCard card;

    public void Reset()
    {
        IsValid = false;
        if (playQuery.virtualCards.Count == 0) return;
        foreach (var i in handCards.Where(x => x.model.isVirtual).ToList())
        {
            handCards.Remove(i);
            Destroy(i.gameObject);
        }
        CardArea.Instance.MoveAll(0.1f);
    }

    private void Reset(FinishPlay finishPlay)
    {
        if (Player.IsSelf(finishPlay.player)) Reset();
    }

    public void OnClickCard(int id, bool value)
    {
        if (value)
        {
            // 若已选中手牌数量超出范围，取消第一张选中的手牌
            var card = handCards.Find(x => x.id == decision.virtualCard);
            decision.virtualCard = id;
            if (card != null)
            {
                card.toggle.isOn = false;
                // return;
            }
        }
        else if (decision.virtualCard == id) decision.virtualCard = 0;
        else return;

        // decision.virtualCard = value ? id : 0;

        // 判断是否已符合要求
        IsValid = decision.virtualCard != 0;

        DestArea.Instance.Reset();
        DestArea.Instance.OnStartPlay();
        PlayArea.Instance.UpdateButtonArea();
    }
}