using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using NativeWebSocket;

namespace View
{
    public class SgsStart : SingletonMono<SgsStart>
    {
        public GameObject startPanel;
        public GameObject signInPanel;
        public Button start;
        public Button signOut;

        public Text nickname;

        private void Start()
        {
            start.onClick.AddListener(ClickStart);
            signOut.onClick.AddListener(ClickSignOut);

            BGM.Instance.Load(Url.AUDIO + "bgm/outbgm_2.mp3");
        }

        public void ShowStartPanel()
        {
            signInPanel.SetActive(false);
            startPanel.SetActive(true);
            nickname.text = SignIn.Instance.username.text;
        }

        private void ClickStart()
        {
            Model.Room.Instance.IsSingle = false;
            SceneManager.Instance.LoadScene("Lobby");
        }

        private void ClickSignOut()
        {
            startPanel.SetActive(false);
            signInPanel.SetActive(true);
        }
    }
}