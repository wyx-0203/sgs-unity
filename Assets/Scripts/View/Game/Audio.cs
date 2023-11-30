using System.Collections.Generic;
using UnityEngine;

public class Audio : SingletonMono<Audio>
{
    public AudioSource effect;

    private void Start()
    {
        GameCore.Card.UseCardView += CardVoice;
        // GameCore.UpdateHp.ActionView += OnDamage;
        GameCore.Skill.UseSkillView += SkillVoice;
        GameCore.SetLock.ActionView += OnLock;
    }

    private void OnDestroy()
    {
        GameCore.Card.UseCardView -= CardVoice;
        // GameCore.UpdateHp.ActionView -= OnDamage;
        GameCore.Skill.UseSkillView -= SkillVoice;
        GameCore.SetLock.ActionView -= OnLock;
    }

    private Dictionary<System.Type, int> urls = new Dictionary<System.Type, int>
    {
        { typeof(GameCore.杀), 1 },
        { typeof(GameCore.闪), 2 },
        { typeof(GameCore.桃), 3 },
        { typeof(GameCore.顺手牵羊), 4 },
        { typeof(GameCore.过河拆桥), 5 },
        { typeof(GameCore.无中生有), 7 },
        { typeof(GameCore.决斗), 8 },
        { typeof(GameCore.南蛮入侵), 9 },
        { typeof(GameCore.万箭齐发), 10 },
        { typeof(GameCore.闪电), 11 },
        { typeof(GameCore.桃园结义), 12 },
        { typeof(GameCore.无懈可击), 13 },
        { typeof(GameCore.借刀杀人), 14 },
        { typeof(GameCore.乐不思蜀), 15 },
        // { typeof(Model.SubHorse), 18 },
        // { typeof(Model.PlusHorse), 18 },
        { typeof(GameCore.火杀), 16 },
        { typeof(GameCore.雷杀), 17 },
        { typeof(GameCore.酒), 82 },
        { typeof(GameCore.火攻), 83 },
        { typeof(GameCore.兵粮寸断), 84 },
        { typeof(GameCore.铁索连环), 85 },
    };

    private async void CardVoice(GameCore.Card card)
    {
        string url = Url.AUDIO + "spell/";

        if (!urls.ContainsKey(card.GetType()))
        {
            if (card is GameCore.Weapon || card is GameCore.Armor)
            {
                url += "equipArmor.mp3";
            }
            else if (card is GameCore.SubHorse || card is GameCore.PlusHorse)
            {
                url += "equipHorse.mp3";
            }
            else return;
        }
        else
        {
            int gender = card.src.general.gender ? 1 : 2;
            url += "spell" + urls[card.GetType()] + "_" + gender + ".mp3";
        }

        effect.PlayOneShot(await WebRequest.GetClip(url));
    }

    // private async void OnDamage(GameCore.UpdateHp model)
    // {
    //     if (model is not GameCore.Damaged damaged) return;
    //     var type = damaged.type;
    //     string url = type == GameCore.Damaged.Type.Thunder ? "spell86_1" : type == GameCore.Damaged.Type.Fire ? "spell87_1" : "hurtSound";
    //     url = Url.AUDIO + "spell/" + url + ".mp3";
    //     effect.PlayOneShot(await WebRequest.GetClip(url));
    // }

    private async void SkillVoice(GameCore.Skill model, List<GameCore.Player> dests)
    {
        try
        {
            var voice = model.src.currentSkin.voice.Find(x => x.name == model.name)?.url;
            string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)];
            effect.PlayOneShot(await WebRequest.GetClip(url));
        }
        catch (System.Exception e) { Debug.Log(e); }
    }

    private async void OnLock(GameCore.SetLock model)
    {
        string url = model.ByDamage ? "tiesuolianhuan_clear" : "tiesuolianhuan";
        url = Url.AUDIO + "spell/" + url + ".mp3";
        effect.PlayOneShot(await WebRequest.GetClip(url));
    }
}