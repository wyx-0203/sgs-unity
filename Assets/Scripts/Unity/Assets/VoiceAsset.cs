using System.Collections.Generic;
using UnityEngine;

public class VoiceAsset : ScriptableObject
{
    public List<KeyValue<List<AudioClip>>> voices;

    public AudioClip GetRandomSkill(string skill)
    {
        var list = voices.Get(skill);
        if (list is null) return null;
        return list[Random.Range(0, list.Count)];
    }
}
