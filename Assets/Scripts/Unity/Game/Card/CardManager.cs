using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CardManager : SingletonMono<CardManager>
{
    // 存放场上除手牌区和弃牌区以外的卡牌
    private readonly Dictionary<int, Card> movingCards = new();

    private int self => Game.Instance.firstPerson.model.index;

    private void Start()
    {
        EventSystem.Instance.AddEvent<AddToDiscard>(OnAddToDiscard);

        EventSystem.Instance.AddEvent<DrawCard>(OnDrawCard);
        EventSystem.Instance.AddEvent<GetAnothersCard>(OnGetAnothersCard);
        EventSystem.Instance.AddEvent<GetCardInJudgeArea>(OnGetCardInJudgeArea);
        EventSystem.Instance.AddEvent<GetDiscard>(OnGetDiscard);
        EventSystem.Instance.AddEvent<ShowCard>(OnShowCard);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<AddToDiscard>(OnAddToDiscard);

        EventSystem.Instance.RemoveEvent<DrawCard>(OnDrawCard);
        EventSystem.Instance.RemoveEvent<GetAnothersCard>(OnGetAnothersCard);
        EventSystem.Instance.RemoveEvent<GetCardInJudgeArea>(OnGetCardInJudgeArea);
        EventSystem.Instance.RemoveEvent<GetDiscard>(OnGetDiscard);
        EventSystem.Instance.RemoveEvent<ShowCard>(OnShowCard);
    }

    /// <summary>
    /// 返回带有布局组的卡组对象，用于动画系统中存放一张或多张卡牌，如摸两张牌时需将两张牌设为卡组子对象
    /// </summary>
    private Transform NewCardGroup(Vector3 position)
    {
        var cardGroup = Instantiate(GameAsset.Instance.cardGroup, transform);
        cardGroup.position = position;
        Destroy(cardGroup.gameObject, 0.6f);
        return cardGroup;
    }

    /// <summary>
    /// 初始化卡组位置
    /// </summary>
    private async Task UpdateCardGroup()
    {
        // 等待一帧，使所有Card.target.transform的改变生效 (通常 SetParent(cardGroup) 后需调用此方法)
        await Util.WaitFrame();

        // 初始化卡牌位置 (使卡组中所有卡牌实体的位置等于目标位置)
        foreach (var i in movingCards.Values) i.Move(0);
    }

    /// <summary>
    /// 移动场上所有卡牌
    /// </summary>
    private async Task MoveAll(float second)
    {
        CardArea.Instance.MoveAll(second);
        DiscardPile.Instance.MoveAll(second);

        await Util.WaitFrame();
        foreach (var i in movingCards.Values) i.Move(second);
    }

    /// <summary>
    /// 武将位置
    /// </summary>
    private Vector3 GetPos(int player)
    {
        if (player == self) return Self.Instance.transform.position;
        else return Game.Instance.players.Find(x => x.model.index == player).transform.position;
    }

    /// <summary>
    /// 手牌区卡牌位置
    /// </summary>
    private Vector3 SelfCardPos(int id)
    {
        var card = CardArea.Instance.handCards.Find(x => x.model.id == id)?.transform;
        if (card is null) card = EquipArea.Instance.equipments.Values.FirstOrDefault(x => x.id == id)?.transform;
        return card != null ? card.position : Self.Instance.transform.position;
        // if (self.handCards.Contains(model)) return CardArea.Instance.handcards.Find(x => x.model == model).transform.position;
        // else return Self.Instance.transform.position;
    }

    /// <summary>
    /// 添加牌到弃牌堆
    /// </summary>
    private async void AddDiscard(List<int> cards, int src)
    {
        // var src = cards[0].src;

        // 若无来源(如判定牌)
        if (src == -1)
        {
            foreach (int i in cards) DiscardPile.Instance.Add(Card.New(i, true));
        }

        // 若卡牌来源为自己
        else if (src == self)
        {
            foreach (var i in cards)
            {
                var card = Card.New(i, true);

                // 将卡牌起点设为手牌区
                card.transform.position = SelfCardPos(i);

                // 终点为弃牌区
                DiscardPile.Instance.Add(card);
            }
        }

        // 若为其他角色
        else
        {
            // 卡牌起点为武将位置
            var cardGroup = NewCardGroup(GetPos(src));
            foreach (var i in cards)
            {
                var card = Card.New(i, true);
                card.SetParent(cardGroup);
                movingCards.Add(i, card);
            }

            // 等待一帧，使所有transform生效
            await UpdateCardGroup();

            // 设置终点
            foreach (var i in cards)
            {
                DiscardPile.Instance.Add(movingCards[i]);
                movingCards.Remove(i);
            }

        }

        // 开始移动
        await MoveAll(0.3f);
    }

    private void OnAddToDiscard(AddToDiscard addToDiscard)
    {
        AddDiscard(addToDiscard.cards, addToDiscard.player);
    }

    private void OnShowCard(ShowCard showCard)
    {
        AddDiscard(showCard.cards, showCard.player);
    }

    /// <summary>
    /// 摸牌
    /// </summary>
    private async void OnDrawCard(DrawCard drawCard)
    {
        var destPos = GetPos(drawCard.player);
        var srcPos = Vector3.MoveTowards(destPos, transform.position, 50);
        var cardGroup = NewCardGroup(srcPos);

        // 设置卡牌起点(武将位置向屏幕中心偏移50个单位)
        foreach (var i in drawCard.cards)
        {
            var card = drawCard.player == self ? Card.NewHandCard(i) : Card.New(i, Player.Find(drawCard.player).model.isSelf);
            card.SetParent(cardGroup);
            movingCards.Add(i, card);
        }

        await UpdateCardGroup();

        // 若卡牌来源为自己，则加入手牌区
        if (drawCard.player == self)
        {
            foreach (var i in drawCard.cards)
            {
                CardArea.Instance.Add(movingCards[i]);
                movingCards.Remove(i);
            }
            await MoveAll(0.3f);
        }

        // 若为其他角色，则终点为武将位置
        else
        {
            cardGroup.position = destPos;
            await MoveAll(0.3f);
            foreach (var i in drawCard.cards) movingCards.Remove(i);
        }
    }

    /// <summary>
    /// 获得其他角色的牌
    /// </summary>
    private async void OnGetAnothersCard(GetAnothersCard gac)
    {
        int player = gac.player;
        int src = gac.dest;
        var cards = gac.cards;
        var known = gac.known;

        var cardGroup = NewCardGroup(GetPos(src));

        // 若获得牌的角色为自己
        if (player == self)
        {
            // 起点为卡牌来源的位置
            foreach (var i in cards)
            {
                var card = Card.NewHandCard(i);
                while (movingCards.ContainsKey(i)) await Task.Yield();
                movingCards.Add(i, card);
                card.SetParent(cardGroup);
            }

            await UpdateCardGroup();

            // 终点为手牌区
            foreach (var i in cards)
            {
                CardArea.Instance.Add(movingCards[i]);
                movingCards.Remove(i);
            }

            await MoveAll(0.3f);
            return;
        }

        // 若卡牌来源为自己
        else if (src == self)
        {
            // 起点为手牌区
            foreach (var i in cards)
            {
                var card = Card.New(i, true);
                card.transform.position = SelfCardPos(i);

                card.SetParent(cardGroup);
                movingCards.Add(i, card);
            }
        }

        // 来源与目标都不是自己
        else
        {
            // 起点为来源位置
            for (int i = 0; i < cards.Count; i++)
            {
                var card = Card.New(cards[i], known[i] || Player.Find(src).model.isSelf || Player.Find(player).model.isSelf);
                card.SetParent(cardGroup);
                movingCards.Add(cards[i], card);
            }

            await UpdateCardGroup();
        }

        // 终点为目标武将位置
        cardGroup.position = GetPos(player);

        await MoveAll(0.3f);
        foreach (var i in cards) movingCards.Remove(i);
    }

    private void OnGetCardInJudgeArea(GetCardInJudgeArea getJudgeCard)
    {
        OnGetAnothersCard(new GetAnothersCard
        {
            player = getJudgeCard.player,
            dest = getJudgeCard.dest,
            cards = getJudgeCard.cards,
            known = getJudgeCard.cards.Select(x => true).ToList()
        });
    }

    /// <summary>
    /// 从弃牌堆获得牌 (奸雄)
    /// </summary>
    private async void OnGetDiscard(GetDiscard model)
    {
        // if (model is not GameCore.GetDisCard) return;

        if (model.player == self)
        {
            foreach (var i in model.cards)
            {
                var card = Card.NewHandCard(i);

                // 起点为弃牌位置
                var discard = DiscardPile.Instance.Cards.Find(x => x.id == i);
                if (discard != null) card.transform.position = discard.transform.position;

                // 终点为手牌区
                CardArea.Instance.Add(card);
            }

            await MoveAll(0.3f);
        }

        else
        {
            var cardGroup = NewCardGroup(GetPos(model.player));
            foreach (var i in model.cards)
            {
                var card = Card.New(i, true);

                // 起点为弃牌位置
                var discard = DiscardPile.Instance.Cards.Find(x => x.id == i);
                if (discard != null) card.transform.position = discard.transform.position;

                // 终点为武将位置
                card.SetParent(cardGroup);
                movingCards.Add(i, card);
            }

            await MoveAll(0.3f);
            foreach (var i in model.cards) movingCards.Remove(i);
        }
    }
}