using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using NativeWebSocket;

public class SgsStart : SingletonMono<SgsStart>
{
    public GameObject startPanel;
    public Button start;
    public Button signOut;

    public Text nickname;

    private void Start()
    {
        start.onClick.AddListener(ClickStart);
        signOut.onClick.AddListener(ClickSignOut);

        // BGM.Instance.Load(Url.AUDIO + "bgm/outbgm_2.mp3");
    }

    public void ShowStartPanel()
    {
        // SignIn.Instance.gameObject.SetActive(false);
        startPanel.SetActive(true);
        nickname.text = SignIn.Instance.username.text;
    }

    private void ClickStart()
    {
        // GameCore.Room.Instance.IsSingle = false;
        SceneManager.Instance.LoadScene("Lobby");
    }

    private void ClickSignOut()
    {
        startPanel.SetActive(false);
        SignIn.Instance.gameObject.SetActive(true);
    }
}