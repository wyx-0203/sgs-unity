using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class GeneralBP : MonoBehaviour
    {
        public int Id { get; private set; }
        private General general;
        private Model.General model;

        public Button button;
        public GameObject ban;
        // 敌方已选
        public GameObject oppoPicked;

        public bool IsSelf { get; private set; }

        private void Start()
        {
            button.onClick.AddListener(SetBpResult);
        }

        public void Init(Model.General model)
        {
            this.model = model;
            Id = model.id;

            general = GetComponent<General>();
            general.Init(model);
        }

        public void OnBan()
        {
            general.skin.color = new Color(0.4f, 0.4f, 0.4f);
            ban.SetActive(true);
        }

        public void OnPick(bool isSelf)
        {
            IsSelf = isSelf;
            if (!IsSelf)
            {
                general.skin.color = new Color(0.4f, 0.4f, 0.4f);
                oppoPicked.SetActive(true);
            }
        }

        public void SetBpResult()
        {
            button.interactable = false;
            Model.BanPick.Instance.SendBpResult(Id);
        }

        private Transform parent;

        public void ToSelfPick()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Select);
            button.interactable = true;
            parent = transform.parent;
            rectTransform = GetComponent<RectTransform>();
            originPos = rectTransform.anchoredPosition;
        }

        private RectTransform rectTransform;
        private Vector2 originPos;
        private bool isSelect;

        public void Select()
        {
            if (!isSelect)
            {
                if (SelfPick.Instance.general0 is null)
                {
                    transform.SetParent(SelfPick.Instance.seat0);
                    SelfPick.Instance.general0 = this;
                }
                else if (SelfPick.Instance.general1 is null)
                {
                    transform.SetParent(SelfPick.Instance.seat1);
                    SelfPick.Instance.general1 = this;
                }
                else return;
                rectTransform.anchoredPosition = new Vector2(0, 7);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                isSelect = true;
            }
            else
            {
                if (SelfPick.Instance.general0 == this) SelfPick.Instance.general0 = null;
                else SelfPick.Instance.general1 = null;

                transform.SetParent(parent);

                rectTransform.anchoredPosition = originPos;
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchorMin = new Vector2(0, 1);
                isSelect = false;
            }
            SelfPick.Instance.UpdateButton();
        }
    }
}