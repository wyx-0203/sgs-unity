using System.Collections.Generic;
using UnityEngine.UI;

namespace View
{
    public class DiscardArea : SingletonMono<DiscardArea>
    {
        public List<Card> Cards { get; private set; } = new List<Card>();

        private void Start()
        {
            Model.TurnSystem.Instance.FinishPerformView += Clear;
            Model.TurnSystem.Instance.FinishPhaseView += Clear;
        }

        private void OnDestroy()
        {
            Model.TurnSystem.Instance.FinishPerformView -= Clear;
            Model.TurnSystem.Instance.FinishPhaseView -= Clear;
        }

        public void Add(Card card)
        {
            Cards.Add(card);
            card.SetParent(transform);
        }

        public async void Clear(Model.TurnSystem turnSystem)
        {
            foreach (var i in Cards) Destroy(i.gameObject, 2);

            await new Delay(2.1f).Run();
            if (gameObject is null) return;
            MoveAll(0.1f);
        }

        public HorizontalLayoutGroup horizontalLayoutGroup;

        /// <summary>
        /// 弃牌数量达到8时，更新间距
        /// </summary>
        private void UpdateSpacing()
        {
            if (transform.childCount >= 7)
            {
                var spacing = (810 - 121.5f * transform.childCount) / (float)(transform.childCount - 1);
                horizontalLayoutGroup.spacing = spacing;
            }
            else horizontalLayoutGroup.spacing = 0;
        }

        public async void MoveAll(float second)
        {
            UpdateSpacing();

            await Util.Instance.WaitFrame();
            foreach (var i in Cards) i.Move(second);
        }
    }
}
