using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class LeftMenu : SingletonMono<LeftMenu>
    {
        public Button changeSkin;
        public Button changeBg;
        public Button debug;
        // public Button surrender;

        void Start()
        {
            changeSkin.onClick.AddListener(ChangeSkin);
            changeBg.onClick.AddListener(ChangeBg);
            debug.onClick.AddListener(ClickDebug);
            // surrender.onClick.AddListener(ClickSurrender);
        }

        private void ChangeSkin()
        {
            SgsMain.Instance.self.model.SendChangeSkin();
        }

        private void ChangeBg()
        {
            SgsMain.Instance.ChangeBg();
        }

        public void ClickDebug() => Debug.Log(Util.GetGameInfo());

        // private void ClickSurrender()
        // {
        //     Model.GameOver.Instance.SendSurrender();
        //     if (Model.Timer.Instance.isPlayPhase) Model.Timer.Instance.SendDecision();
        // }
    }
}