using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spine.Unity;
using UnityEngine;

[Serializable]
public class SkinAsset
{
    public int id;
    public int generalId;
    public string name;
    public bool dynamic;

    private static List<SkinAsset> list;

    public static async Task Init()
    {
        if (list != null) return;
        string json = await WebRequest.Get(Url.JSON + "skin.json");
        list = JsonConvert.DeserializeObject<List<SkinAsset>>(json);
    }

    public static SkinAsset Get(int id) => list.Find(x => x.id == id);
    public static List<SkinAsset> GetList() => list;

    public async Task<AudioClip> GetVoice(string skill)
    {
        var ab = await ABManager.Instance.Load($"voice/{id}");
        return ab.LoadAsset<VoiceAsset>($"{id}.asset")?.GetRandomSkill(skill);
    }

    public async Task<Sprite> GetSeatImage()
    {
        string url = $"{Url.GENERAL_IMAGE}Seat/{id}.png";
        var texture = await WebRequest.GetTexture(url);
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public async Task<Sprite> GetWindowImage()
    {
        string url = $"{Url.GENERAL_IMAGE}Window/{id}.png";
        var texture = await WebRequest.GetTexture(url);
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public async Task<Sprite> GetBigImage()
    {
        string url = $"{Url.GENERAL_IMAGE}Big/{id}.png";
        var texture = await WebRequest.GetTexture(url);
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public async Task<SkeletonDataAsset> GetGameSkeleton()
    {
        var ab = await ABManager.Instance.Load($"dynamic/{id}");
        return ab.LoadAsset<SkeletonDataAsset>("daiji2_SkeletonData.asset");
    }

    public async Task<SkeletonDataAsset> GetDaijiSkeleton()
    {
        var ab = await ABManager.Instance.Load($"dynamic/{id}");
        return ab.LoadAsset<SkeletonDataAsset>("daiji_SkeletonData.asset");
    }

    public async Task<SkeletonDataAsset> GetBgSkeleton()
    {
        var ab = await ABManager.Instance.Load($"dynamic/{id}");
        return ab.LoadAsset<SkeletonDataAsset>("beijing_SkeletonData.asset");
    }
}
