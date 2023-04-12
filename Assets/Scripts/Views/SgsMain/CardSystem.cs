using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace View
{
    public class CardSystem : SingletonMono<CardSystem>
    {
        public GameObject cardPrefab;
        public GameObject cardGroupPrefab;

        // 存放场上除手牌区和弃牌区以外的卡牌
        private Dictionary<int, Card> movingCards = new Dictionary<int, Card>();

        private Model.Player self => SgsMain.Instance.self.model;

        private void Start()
        {
            Model.CardPile.Instance.DiscardView += AddDiscard;
            Model.ShowCard.ActionView += AddDiscard;

            Model.GetCard.ActionView += GetCard;
            Model.ExChange.ActionView += Exchange;
        }

        private void OnDestroy()
        {
            Model.CardPile.Instance.DiscardView -= AddDiscard;
            Model.ShowCard.ActionView -= AddDiscard;

            Model.GetCard.ActionView -= GetCard;
            Model.ExChange.ActionView -= Exchange;
        }

        /// <summary>
        /// 返回带有布局组的卡组对象，用于动画系统中存放一张或多张卡牌，如摸两张牌时需将两张牌设为卡组子对象
        /// </summary>
        private Transform NewCardGroup(Vector3 position)
        {
            var cardGroup = Instantiate(cardGroupPrefab, transform).transform;
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
            await Util.Instance.WaitFrame();

            // 初始化卡牌位置 (使卡组中所有卡牌实体的位置等于目标位置)
            foreach (var i in movingCards.Values) i.Move(0);
        }

        /// <summary>
        /// 移动场上所有卡牌
        /// </summary>
        private async Task MoveAll(float second)
        {
            CardArea.Instance.MoveAll(second);
            DiscardArea.Instance.MoveAll(second);

            await Util.Instance.WaitFrame();
            foreach (var i in movingCards.Values) i.Move(second);
        }

        /// <summary>
        /// 武将位置
        /// </summary>
        private Vector3 Pos(Model.Player model)
        {
            if (model == self) return Self.Instance.transform.position;
            else return SgsMain.Instance.players[model.position].transform.position;
        }

        /// <summary>
        /// 手牌区卡牌位置
        /// </summary>
        private Vector3 SelfCardPos(Model.Card model)
        {
            if (self.HandCards.Contains(model)) return CardArea.Instance.handcards[model.Id].transform.position;
            else return Self.Instance.transform.position;
        }

        /// <summary>
        /// 添加牌到弃牌堆
        /// </summary>
        private async void AddDiscard(List<Model.Card> cards)
        {
            var src = cards[0].Src;

            // 若无来源(如判定牌)
            if (src is null)
            {
                foreach (var i in cards) DiscardArea.Instance.Add(Card.New(i, true));
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
                    DiscardArea.Instance.Add(card);
                }
            }

            // 若为其他角色
            else
            {
                // 卡牌起点为武将位置
                var cardGroup = NewCardGroup(Pos(src));
                foreach (var i in cards)
                {
                    var card = Card.New(i, true);
                    card.SetParent(cardGroup);
                    movingCards.Add(i.Id, card);
                }

                // 等待一帧，使所有transform生效
                await UpdateCardGroup();

                // 设置终点
                foreach (var i in cards)
                {
                    DiscardArea.Instance.Add(movingCards[i.Id]);
                    movingCards.Remove(i.Id);
                }

            }

            // 开始移动
            await MoveAll(0.3f);
        }

        private void AddDiscard(Model.ShowCard model)
        {
            AddDiscard(model.Cards);
        }

        /// <summary>
        /// 摸牌
        /// </summary>
        private async void GetCardFromPile(Model.GetCard model)
        {
            var destPos = Pos(model.player);
            var srcPos = Vector3.MoveTowards(destPos, transform.position, 50);
            var cardGroup = NewCardGroup(srcPos);

            // 设置卡牌起点(武将位置向屏幕中心偏移50个单位)
            foreach (var i in model.Cards)
            {
                var card = model.player == self ? Card.NewHandCard(i) : Card.New(i, model.player.isSelf);
                card.SetParent(cardGroup);
                movingCards.Add(i.Id, card);
            }

            await UpdateCardGroup();

            // 若卡牌来源为自己，则加入手牌区
            if (model.player == self)
            {
                foreach (var i in model.Cards)
                {
                    CardArea.Instance.Add(movingCards[i.Id]);
                    movingCards.Remove(i.Id);
                }
                await MoveAll(0.3f);
            }

            // 若为其他角色，则终点为武将位置
            else
            {
                cardGroup.position = destPos;
                await MoveAll(0.3f);
                foreach (var i in model.Cards) movingCards.Remove(i.Id);
            }
        }

        /// <summary>
        /// 获得其他角色的牌
        /// </summary>
        private async void GetCardFromElse(List<Model.Card> cards, Model.Player src, Model.Player dest, bool known)
        {
            var cardGroup = NewCardGroup(Pos(src));

            // 若获得牌的角色为自己
            if (dest == self)
            {
                // 起点为卡牌来源的位置
                foreach (var i in cards)
                {
                    var card = Card.NewHandCard(i);
                    movingCards.Add(i.Id, card);
                    card.SetParent(cardGroup);
                }

                await UpdateCardGroup();

                // 终点为手牌区
                foreach (var i in cards)
                {
                    CardArea.Instance.Add(movingCards[i.Id]);
                    movingCards.Remove(i.Id);
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
                    movingCards.Add(i.Id, card);
                }
            }

            // 来源与目标都不是自己
            else
            {
                // 起点为来源位置
                foreach (var i in cards)
                {
                    var card = Card.New(i, known);
                    card.SetParent(cardGroup);
                    movingCards.Add(i.Id, card);
                }

                await UpdateCardGroup();
            }

            // 终点为目标武将位置
            cardGroup.position = Pos(dest);

            await MoveAll(0.3f);
            foreach (var i in cards) movingCards.Remove(i.Id);
        }

        private void GetCardFromElse(Model.GetCardFromElse model)
        {
            GetCardFromElse(model.Cards, model.Dest, model.player, model.Dest.isSelf || model.player.isSelf);
        }

        private void Exchange(Model.ExChange model)
        {
            bool known = model.Dest.isSelf || model.player.isSelf;
            GetCardFromElse(model.Dest.HandCards, model.Dest, model.player, known);
            GetCardFromElse(model.player.HandCards, model.player, model.Dest, known);
        }

        private void GetJudgeCard(Model.GetCard model)
        {
            if (model is not Model.GetJudgeCard) return;
            GetCardFromElse(model.Cards, (model.Cards[0] as Model.DelayScheme).Owner, model.player, true);
        }

        /// <summary>
        /// 从弃牌堆获得牌 (如奸雄)
        /// </summary>
        private async void GetDiscard(Model.GetCard model)
        {
            if (model is not Model.GetDisCard) return;

            if (model.player == self)
            {
                foreach (var i in model.Cards)
                {
                    var card = Card.NewHandCard(i);

                    // 起点为弃牌位置
                    var discard = DiscardArea.Instance.Cards.Find(x => x.Id == i.Id);
                    if (discard != null) card.transform.position = discard.transform.position;

                    // 终点为手牌区
                    CardArea.Instance.Add(card);
                }

                await MoveAll(0.3f);
            }

            else
            {
                var cardGroup = NewCardGroup(Pos(model.player));
                foreach (var i in model.Cards)
                {
                    var card = Card.New(i, true);

                    // 起点为弃牌位置
                    var discard = DiscardArea.Instance.Cards.Find(x => x.Id == i.Id);
                    if (discard != null) card.transform.position = discard.transform.position;

                    // 终点为武将位置
                    card.SetParent(cardGroup);
                    movingCards.Add(i.Id, card);
                }

                await MoveAll(0.3f);
                foreach (var i in model.Cards) movingCards.Remove(i.Id);
            }
        }

        private void GetCard(Model.GetCard model)
        {
            if (model is Model.GetCardFromPile) GetCardFromPile(model);
            else if (model is Model.GetCardFromElse) GetCardFromElse(model as Model.GetCardFromElse);
            else if (model is Model.GetJudgeCard) GetJudgeCard(model);
            else if (model is Model.GetDisCard) GetDiscard(model);
        }
    }
}
