using System;
using System.Collections;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class GameInfo : MonoBehaviour
{
    public Text pileCount;
    public Text time;
    public Text round;
    public DateTime startFightTime;

    private void Awake()
    {
        EventSystem.Instance.AddEvent<DrawCard>(OnUpdatePileCount);
        EventSystem.Instance.AddEvent<FinishBanPick>(OnStartFight);
        EventSystem.Instance.AddEvent<NewRound>(OnNewRound);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<DrawCard>(OnUpdatePileCount);
        EventSystem.Instance.RemoveEvent<FinishBanPick>(OnStartFight);
        EventSystem.Instance.RemoveEvent<NewRound>(OnNewRound);
    }

    private void OnUpdatePileCount(DrawCard updatePileCount)
    {
        pileCount.text = $"牌堆: {updatePileCount.pileCount}";
    }

    private void OnStartFight(FinishBanPick finishBanPick)
    {
        startFightTime = finishBanPick.startFightTime;
        StartCoroutine(UpdateTime());
    }

    private IEnumerator UpdateTime()
    {
        while (true)
        {
            time.text = $"{DateTime.Now - startFightTime:mm\\.ss}";
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnNewRound(NewRound newRound)
    {
        round.text = $"第{newRound.round}轮";
    }
}