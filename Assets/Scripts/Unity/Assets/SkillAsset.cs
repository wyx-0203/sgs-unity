using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class SkillAsset
{
    public enum Type
    {
        [InspectorName("主动技")] Normal,
        [InspectorName("锁定技")] Passive,
        [InspectorName("限定技")] Limited,
        [InspectorName("觉醒技")] Awoken,
        [InspectorName("主公技")] Monarch
    }
    public string name;
    public Type type;
    [TextArea(8, 10)][InspectorName("描述")] public string describe;

    private static List<SkillAsset> list;
    public static async Task Init()
    {
        if (list != null) return;
        string json = await WebRequest.Get(Url.JSON + "skill.json");
        list = JsonConvert.DeserializeObject<List<SkillAsset>>(json);
    }
    public static SkillAsset Get(string name) => list.Find(x => x.name == name);
    public static List<SkillAsset> GetList() => list;
}
