using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class PanelCard : MonoBehaviour
    {
        private Card card;

        public void Init()
        {
            card = GetComponent<Card>();
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.interactable = true;
        }

        private void OnClick()
        {
            CardPanel.Instance.selectCard = card;
            CardPanel.Instance.UpdatePanel();
        }
    }
}