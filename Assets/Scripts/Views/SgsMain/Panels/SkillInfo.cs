using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SkillInfo : MonoBehaviour
    {
        public Text title;
        public Text discribe;
        public Button voice;

        private void Start()
        {
            if (voice != null) voice.onClick.AddListener(Onclick);
        }

        private void Onclick()
        {
            GeneralDetail.Instance.SkillVoice(title.text);
        }
    }
}
