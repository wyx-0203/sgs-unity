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
        // GameMain.Instance.self.model.SendChangeSkin();
        var model = GameMain.Instance.firstPerson.model;
        int index = (model.skins.IndexOf(model.currentSkin) + 1) % model.skins.Count;
        var skin = model.skins[index];
        EventSystem.Instance.Send(new Model.ChangeSkin
        {
            player = model.index,
            skinId = skin.id
        });
    }

    private void ChangeBg()
    {
        GameMain.Instance.ChangeBg();
    }

    // public void ClickDebug() => Debug.Log(Util.GetGameInfo());

    private void ClickSurrender()
    {
        EventSystem.Instance.Send(new Model.Surrender { player = GameMain.Instance.firstPerson.model.index });
        // GameCore.Main.Instance.SendSurrender();
        // GameCore.Timer.Instance.SendDecision();
    }
}