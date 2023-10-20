using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class GeneralBP : MonoBehaviour
    {
        public enum State
        {
            Self,
            Enemy,
            Ban,
            Selectable
        }

        // public int Id { get; private set; }
        private General general;
        public Model.General model => general.model;

        public Button button;
        public GameObject ban;
        // 敌方已选
        public GameObject oppoPicked;
        // public bool isOppo=>oppoPicked.activeSelf;

        // public bool IsSelf { get; private set; }
        public State state { get; private set; } = State.Selectable;

        private Transform pool => BanPick.Instance.pool;

        public void Init(Model.General model)
        {
            // this.model = model;
            // Id = model.id;

            general = GetComponent<General>();
            general.Init(model);

            button.onClick.AddListener(SetBpResult);
        }

        public void OnBan()
        {
            state = State.Ban;
            general.skin.color = new Color(0.4f, 0.4f, 0.4f);
            ban.SetActive(true);
        }

        public void OnPick(bool isSelf)
        {
            // IsSelf = isSelf;
            if (!isSelf)
            {
                state = State.Enemy;
                general.skin.color = new Color(0.4f, 0.4f, 0.4f);
                oppoPicked.SetActive(true);
            }
            else
            {
                state = State.Self;
                transform.SetParent(BanPick.Instance.selfPool);
            }
        }

        public void SetBpResult()
        {
            button.interactable = false;
            Model.BanPick.Instance.SendBpResult(model.id);
        }

        // private Transform parent;

        public async void ToSelfPick()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            button.interactable = true;
            transform.SetParent(pool);
            rectTransform = GetComponent<RectTransform>();

            await Util.WaitFrame();
            originPos = rectTransform.anchoredPosition;
        }

        private RectTransform rectTransform;
        private Vector2 originPos;
        private bool isSelect;

        public void OnClick()
        {
            if (!isSelect)
            {
                var seat = BanPick.Instance.seats.FirstOrDefault(x => x.general is null);
                if (seat is null) return;

                transform.SetParent(seat.transform);
                seat.general = this;
                // if (SelfPick.Instance.general0 is null)
                // {
                //     transform.SetParent(SelfPick.Instance.seat0);
                //     SelfPick.Instance.general0 = this;
                // }
                // else if (SelfPick.Instance.general1 is null)
                // {
                //     transform.SetParent(SelfPick.Instance.seat1);
                //     SelfPick.Instance.general1 = this;
                // }
                // else return;
                rectTransform.anchoredPosition = new Vector2(0, 7);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                isSelect = true;
            }
            else
            {
                var seat = transform.parent.GetComponent<SelfPickSeat>();
                seat.general = null;
                // if (SelfPick.Instance.general0 == this) SelfPick.Instance.general0 = null;
                // else SelfPick.Instance.general1 = null;

                transform.SetParent(pool);

                rectTransform.anchoredPosition = originPos;
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchorMin = new Vector2(0, 1);
                isSelect = false;
            }
            BanPick.Instance.UpdateCommitButton();
        }
    }
}