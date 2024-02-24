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
        var player = Game.Instance.firstPerson;
        var skins = player.model.general.skins;
        int index = (skins.IndexOf(player.skin.asset.id) + 1) % skins.Count;
        EventSystem.Instance.SendToServer(new Model.ChangeSkin
        {
            player = player.model.index,
            skinId = skins[index]
        });
    }

    private void ChangeBg()
    {
        Game.Instance.ChangeBg();
    }

    // public void ClickDebug() => Debug.Log(Util.GetGameInfo());

    private void ClickSurrender()
    {
        EventSystem.Instance.SendToServer(new Model.Surrender { team = GameModel.Instance.selfTeam });
        // GameCore.Main.Instance.SendSurrender();
        // GameCore.Timer.Instance.SendDecision();
    }
}