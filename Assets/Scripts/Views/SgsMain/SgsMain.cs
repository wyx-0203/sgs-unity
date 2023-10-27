using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SgsMain : SingletonMono<SgsMain>
    {
        public RawImage background;

        public List<Player> players { get; private set; } = new();
        public Player self;
        public GameObject gameOver;
        public RectTransform border;
        public GameObject bp;

        public GameObject cardPanel;

        public GameObject playerPrefab;
        public Transform _selfs;
        public Transform _enemys;

        protected override void Awake()
        {
            SetBorder();
            base.Awake();
#if UNITY_EDITOR
            ABManager.Instance.LoadSgsMain();
#elif UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }

        private async void Start()
        {
            bgIndex = Random.Range(0, bgUrl.Count);
            ChangeBg();
            BGM.Instance.Load(Url.AUDIO + "bgm/bgm_1.mp3");

            Model.CardPanel.Instance.StartTimerView += ShowPanel;
            Model.CardPanel.Instance.StopTimerView += HidePanel;

            Model.BanPick.Instance.ShowPanelView += ShowBP;
            Model.SgsMain.Instance.AfterBanPickView += Init;

            Model.SgsMain.Instance.MoveSeatView += MoveSeat;

            Model.SgsMain.Instance.GameOverView += GameOver;

            await Model.SgsMain.Instance.Init();
            Model.SgsMain.Instance.Run();
        }

        private void OnDestroy()
        {
            Model.CardPanel.Instance.StartTimerView -= ShowPanel;
            Model.CardPanel.Instance.StopTimerView -= HidePanel;

            Model.BanPick.Instance.ShowPanelView -= ShowBP;
            Model.SgsMain.Instance.AfterBanPickView -= Init;

            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;

            Model.SgsMain.Instance.GameOverView -= GameOver;
        }

        private void SetBorder()
        {
            float x = GetComponent<RectTransform>().sizeDelta.x;
            float y = GetComponent<RectTransform>().sizeDelta.y;
            Debug.Log("canvas.x = " + x);
            float d = x / y > 2 ? x * 0.5f - y : 0;
            border.offsetMin = new Vector2(d, 0);
            border.offsetMax = new Vector2(-d, 0);
        }

        private void ShowBP()
        {
            bp.SetActive(true);
        }

        /// <summary>
        /// 初始化每个View.Player
        /// </summary>
        private void Init()
        {
            Destroy(bp);
            border.gameObject.SetActive(true);

            foreach (var i in Model.SgsMain.Instance.players)
            {
                var player = Instantiate(playerPrefab).GetComponent<Player>();
                player.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                player.Init(i);
                players.Add(player);
                player.transform.SetParent(i.isSelf ? _selfs : _enemys, false);
            }

            self.Init(Model.SgsMain.Instance.AlivePlayers.Find(x => x.isSelf));
        }

        private void GameOver()
        {
            gameOver.SetActive(true);
        }

        /// <summary>
        /// 更新座位
        /// </summary>
        private void MoveSeat(Model.Player model)
        {
            self.Init(model);
        }

        private void ShowPanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;
            cardPanel.SetActive(true);
        }

        private void HidePanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;
            cardPanel.SetActive(false);
        }

        private List<string> bgUrl = new List<string>
        {
            "10",
            "autoChessbeijing_s",
            "boyunjianri_s",
            "chengneidenghuo_s",
            "qunxiongbeijing_s",
            "shuguobeijing_s",
            "weiguobeijing_s",
            "wuguobeijing_s",
            "zhanchangbeijing_s"
        };

        private int bgIndex;

        public async void ChangeBg()
        {
            string url = Url.IMAGE + "Background/" + bgUrl[bgIndex++ % bgUrl.Count] + ".jpeg";
            background.texture = await WebRequest.GetTexture(url);

            // 调整原始图像大小，以使其像素精准。
            background.SetNativeSize();
            // 适应屏幕
            Texture texture = background.texture;
            Vector2 canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
            float radio = Mathf.Max(canvasSize.x / texture.width, canvasSize.y / texture.height);
            background.rectTransform.sizeDelta *= radio;
        }
    }
}