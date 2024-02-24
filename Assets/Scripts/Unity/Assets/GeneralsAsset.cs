using System.Collections.Generic;
using UnityEngine;

public class GeneralsAsset : ScriptableSingleton<GeneralsAsset>
{
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
