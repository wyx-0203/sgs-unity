using UnityEngine;
using UnityEngine.UI;

public class LeftMenu : SingletonMono<LeftMenu>
{
    public Button changeSkin;
    public Button changeBg;
    public Button debug;
    public Button surrender;

    void Start()
    {
        changeSkin.onClick.AddListener(ChangeSkin);
        changeBg.onClick.AddListener(ChangeBg);
        // debug.onClick.AddListener(ClickDebug);
        surrender.onClick.AddListener(ClickSurrender);
    }

    private void ChangeSkin()
    {
        GameMain.Instance.self.model.SendChangeSkin();
    }

    private void ChangeBg()
    {
        GameMain.Instance.ChangeBg();
    }

    // public void ClickDebug() => Debug.Log(Util.GetGameInfo());

    private void ClickSurrender()
    {
        GameCore.Main.Instance.SendSurrender();
        GameCore.Timer.Instance.SendDecision();
    }
}