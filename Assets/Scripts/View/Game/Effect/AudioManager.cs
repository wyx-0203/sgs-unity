using Model;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMono<AudioManager>
{
    public AudioSource effect;

    private void Start()
    {
        EventSystem.Instance.AddEvent<UseCard>(CardVoice);
        EventSystem.Instance.AddEvent<UseSkill>(SkillVoice);
        EventSystem.Instance.AddEvent<SetLock>(OnLock);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<UseCard>(CardVoice);
        EventSystem.Instance.RemoveEvent<UseSkill>(SkillVoice);
        EventSystem.Instance.RemoveEvent<SetLock>(OnLock);
    }

    private void CardVoice(UseCard useCard)
    {
        if (useCard.gender) effect.PlayOneShot(GameAssets.Instance.cardMaleSound.Get(useCard.name));
        else effect.PlayOneShot(GameAssets.Instance.cardFemaleSound.Get(useCard.name));
    }

    private async void SkillVoice(UseSkill useSkill)
    {
        try
        {
            var voice = Player.Find(useSkill.player).model.currentSkin.voice.Find(x => x.name == useSkill.name)?.url;
            string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)];
            effect.PlayOneShot(await WebRequest.GetClip(url));
        }
        catch (System.Exception e) { Debug.Log(e); }
    }

    private void OnLock(SetLock setLock)
    {
        if (setLock.byDamage) effect.PlayOneShot(GameAssets.Instance.lockSoundByDamage);
        else effect.PlayOneShot(GameAssets.Instance.lockSound);
    }
}