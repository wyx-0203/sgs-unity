using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SgsMenu : MonoBehaviour
    {
        public Text pileCount;
        public Text frame;

        private void Start()
        {
            Model.CardPile.Instance.PileCountView += UpdatePileCount;

            StartCoroutine(UpdateFrame());
        }

        private void OnDestroy()
        {
            Model.CardPile.Instance.PileCountView -= UpdatePileCount;
        }

        private void UpdatePileCount()
        {
            pileCount.text = "牌堆数" + Model.CardPile.Instance.PileCount.ToString();
        }

        private IEnumerator UpdateFrame()
        {
            while (true)
            {
                frame.text = "FPS: " + 1f / Time.deltaTime;
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
