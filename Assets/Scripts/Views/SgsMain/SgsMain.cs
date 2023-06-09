using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace View
{
    public class SgsMain : SingletonMono<SgsMain>
    {
        public RawImage background;

        public GameObject[] players;
        public Player self { get; private set; }
        public GameObject gameOver;
        public RectTransform border;
        public GameObject bp;

        public GameObject cardPanel;

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

        private void Start()
        {
            bgIndex = Random.Range(0, bgUrl.Count);
            ChangeBg();
            BGM.Instance.Load(Url.AUDIO + "bgm/bgm_1.mp3");

            Model.SgsMain.Instance.PositionView += InitPlayers;

            Model.CardPanel.Instance.StartTimerView += ShowPanel;
            Model.CardPanel.Instance.StopTimerView += HidePanel;

            Model.BanPick.Instance.ShowPanelView += ShowBP;

            Model.SgsMain.Instance.MoveSeatView += MoveSeat;

            Model.GameOver.Instance.GameOverView += GameOver;

            Model.SgsMain.Instance.Run();
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.PositionView -= InitPlayers;

            Model.CardPanel.Instance.StartTimerView -= ShowPanel;
            Model.CardPanel.Instance.StopTimerView -= HidePanel;

            Model.BanPick.Instance.ShowPanelView -= ShowBP;

            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;

            Model.GameOver.Instance.GameOverView -= GameOver;
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
        private void InitPlayers(Model.Player[] model)
        {
            // int i;
            for (int i = 0; i < 4; i++)
            {
                players[i].GetComponent<Player>().Init(model[i]);
            }

            foreach (var i in players)
            {
                if (i.GetComponent<Player>().model.isSelf)
                {
                    MoveSeat(i.GetComponent<Player>().model);
                    // Debug.Log("moveseat");
                    break;
                }
            }
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
            if (self != null)
            {
                self.transform.Find("其他角色").gameObject.SetActive(true);
            }

            self = players[model.position].GetComponent<Player>();
            self.transform.Find("其他角色").gameObject.SetActive(false);

            int i = model.position;
            SelfPos(players[i++]);
            RightPos(players[i++ % 4]);
            TopPos(players[i++ % 4]);
            LeftPos(players[i % 4]);
        }

        private void SelfPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.anchorMin = new Vector2(1, 0);
            // rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-125f, 160);
        }
        private void RightPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            // rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-125f, 150);
        }
        private void TopPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            // rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -150);
        }
        private void LeftPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            // rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = new Vector2(125, 150);
        }

        // private GameObject panel;

        private void ShowPanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;

            // panel = Instantiate(ABManager.Instance.GetGameAsset("CardPanel"));
            // panel.transform.SetParent(transform, false);
            cardPanel.SetActive(true);
        }

        private void HidePanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;
            // Destroy(panel);
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