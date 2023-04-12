using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SelfPick : SingletonMono<SelfPick>
    {
        public GridLayoutGroup gridLayoutGroup;
        public ContentSizeFitter contentSizeFitter;
        public GameObject selfPick;

        private bool selfTeam => Model.Self.Instance.team;

        public GeneralBP general0;
        public GeneralBP general1;
        public Transform seat0;
        public Transform seat1;
        public Image pos0;
        public Image pos1;
        public Sprite[] posSprites;
        public Button commit;
        public GameObject border;

        private async void Start()
        {
            foreach (var i in BanPick.Instance.All)
            {
                if (i.ban.activeSelf) Destroy(i.gameObject);
                else if (i.IsSelf)
                {
                    i.transform.SetParent(gridLayoutGroup.transform);
                    // i.ToSelfPick();
                }
            }

            selfPick.SetActive(true);
            border.SetActive(true);
            commit.onClick.AddListener(OnClick);

            gridLayoutGroup.constraintCount = 5;
            gridLayoutGroup.enabled = true;
            contentSizeFitter.enabled = true;

            await Util.Instance.WaitFrame(2);

            contentSizeFitter.enabled = false;
            gridLayoutGroup.enabled = false;

            pos0.sprite = selfTeam == Team.BLUE ? posSprites[3] : posSprites[1];
            pos1.sprite = selfTeam == Team.BLUE ? posSprites[0] : posSprites[2];

            foreach (var i in BanPick.Instance.All)
            {
                if (i.IsSelf) i.ToSelfPick();
            }
        }

        public void UpdateButton()
        {
            commit.gameObject.SetActive(general0 != null && general1 != null);
        }

        private void OnClick()
        {
            int pos = selfTeam == Team.BLUE ? 3 : 1;
            Model.BanPick.Instance.SendSelfResult(pos, general0.Id);
            Model.BanPick.Instance.SendSelfResult(Util.Instance.TeammatePos(pos), general1.Id);
            Destroy(gameObject);
        }
    }
}
