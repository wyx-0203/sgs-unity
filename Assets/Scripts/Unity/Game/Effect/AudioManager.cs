using Model;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMono<AudioManager>
{
    public AudioSource effect;

    private void Start()
    {
        EventSystem.Instance.AddEvent<UseCard>(OnUseCard);
        EventSystem.Instance.AddEvent<UseSkill>(OnUseSkill);
        EventSystem.Instance.AddEvent<Die>(OnDie);
        EventSystem.Instance.AddEvent<SetLock>(OnLock);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<UseCard>(OnUseCard);
        EventSystem.Instance.RemoveEvent<UseSkill>(OnUseSkill);
        EventSystem.Instance.RemoveEvent<Die>(OnDie);
        EventSystem.Instance.RemoveEvent<SetLock>(OnLock);
    }

    private void OnUseCard(UseCard useCard)
    {
        if (useCard.gender == Gender.Male) effect.PlayOneShot(GameAsset.Instance.cardMaleSound.Get(useCard.name));
        else effect.PlayOneShot(GameAsset.Instance.cardFemaleSound.Get(useCard.name));
    }

    private async void OnUseSkill(UseSkill useSkill)
    {
        // try
        // {
        //     var voice = Player.Find(useSkill.player).model.currentSkin.voice.Find(x => x.name == useSkill.skill)?.url;
        //     string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)];
        //     effect.PlayOneShot(await WebRequest.GetClip(url));
        // }
        // catch (System.Exception e) { Debug.Log(e); }
        var skinAsset = Player.Find(useSkill.player).skin.asset;
        effect.PlayOneShot(await skinAsset.GetVoice(useSkill.skill));
    }

    private async void OnDie(Die die)
    {
        // try
        // {
        //     var voice = Player.Find(useSkill.player).model.currentSkin.voice.Find(x => x.name == useSkill.skill)?.url;
        //     string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)];
        //     effect.PlayOneShot(await WebRequest.GetClip(url));
        // }
        // catch (System.Exception e) { Debug.Log(e); }
        var skinAsset = Player.Find(die.player).skin.asset;
        effect.PlayOneShot(await skinAsset.GetVoice("阵亡"));
    }

    private void OnLock(SetLock setLock)
    {
        if (setLock.byDamage) effect.PlayOneShot(GameAsset.Instance.lockSoundByDamage);
        else effect.PlayOneShot(GameAsset.Instance.lockSound);
    }
}