using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Spine.Unity;

[Serializable]
public class KeyValue<T>
{
    public string key;
    public T value;
}

public static class KeyValueList
{
    // 扩展方法
    public static T Get<T>(this List<KeyValue<T>> list, string key) where T : class => list.Find(x => x.key == key)?.value;

    // public static List<KeyValue<T>> New<T>(Dictionary<string, T> dict)
    // {
    //     return dict.Select(x => new KeyValue<T> { key = x.Key, value = x.Value }).ToList();
    // }
}

[CreateAssetMenu(fileName = "GameAssets")]
public class GameAssets : ScriptableObject
{
    public static GameAssets Instance => GameMain.Instance.gameAssets;

    public Card card;
    public Transform cardGroup;

    public List<KeyValue<Sprite>> cardImage;
    public List<KeyValue<AudioClip>> cardMaleSound;
    public List<KeyValue<AudioClip>> cardFemaleSound;
    public List<KeyValue<SkeletonDataAsset>> cardEffect;
    public List<KeyValue<Sprite>> cardSuit;
    public Sprite[] cardBlackWeight;
    public Sprite[] cardRedWeight;
    public List<KeyValue<Sprite>> selfEquipImage;

    public List<KeyValue<Sprite>> equipImage;
    public List<KeyValue<Sprite>> equipSuit;
    public Sprite[] equipBlackWeight;
    public Sprite[] equipRedWeight;

    public List<KeyValue<Sprite>> judgeCardImage;


    // player
    public GeneralBP general;
    public SelfPickSeat seat;
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

    public GameObject normalDamageEffect;
    public GameObject thunderDamageEffect;
    public GameObject fireDamageEffect;

    public AudioClip lockSound;
    public AudioClip lockSoundByDamage;

    public List<Sprite> background;
    public AudioClip bgm;
}
