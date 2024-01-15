using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SgsMenu : MonoBehaviour
{
    public Text pileCount;
    public Text frame;

    private void Start()
    {
        // GameCore.CardPile.Instance.PileCountView += UpdatePileCount;
        EventSystem.Instance.AddEvent<Model.UpdatePileCount>(OnUpdatePileCount);

        StartCoroutine(UpdateFrame());
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<Model.UpdatePileCount>(OnUpdatePileCount);
        // GameCore.CardPile.Instance.PileCountView -= UpdatePileCount;
    }

    private void OnUpdatePileCount(Model.UpdatePileCount updatePileCount)
    {
        pileCount.text = $"牌堆数: {updatePileCount.count}";
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