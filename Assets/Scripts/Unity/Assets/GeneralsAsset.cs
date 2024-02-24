using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "GeneralsAsset")]
public class GeneralsAsset : ScriptableSingleton<GeneralsAsset>
{

    // player
    // public GeneralBP general;
    // public SelfPickSeat seat;
    // public Sprite[] position;
    // public Sprite[] team;
    public List<KeyValue<Sprite>> kindom;
    public List<KeyValue<Sprite>> kindomBg;
    public Sprite[] blood;
    public Color[] bloodColor;
    public int GetBloodIndex(int hp, int hpLimit)
    {
        var ratio = hp / (float)hpLimit;
        // 红
        if (ratio < 0.34) return 0;
        // 黄
        if (ratio < 0.67) return 1;
        // 绿
        return 2;
    }
}
