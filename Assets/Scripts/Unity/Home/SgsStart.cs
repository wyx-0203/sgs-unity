using UnityEngine;
using UnityEngine.UI;

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

        BGM.Instance.Load(GlobalAsset.Instance.lobbyBgm);
    }

    public void ShowStartPanel()
    {
        startPanel.SetActive(true);
        nickname.text = SignIn.Instance.username.text;
    }

    private void ClickStart()
    {
        SceneManager.Instance.LoadScene("Lobby");
    }

    private void ClickSignOut()
    {
        startPanel.SetActive(false);
        SignIn.Instance.gameObject.SetActive(true);
    }
}