using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class Audio : MonoBehaviour
    {
        public AudioSource effect;

        private void Start()
        {
            Model.Card.UseCardView += CardVoice;
            Model.UpdateHp.ActionView += OnDamage;
            Model.Skill.UseSkillView += SkillVoice;
            Model.SetLock.ActionView += OnLock;
        }

        private void OnDestroy()
        {
            Model.Card.UseCardView -= CardVoice;
            Model.UpdateHp.ActionView -= OnDamage;
            Model.Skill.UseSkillView -= SkillVoice;
            Model.SetLock.ActionView -= OnLock;
        }

        private Dictionary<System.Type, int> urls = new Dictionary<System.Type, int>
        {
            { typeof(Model.杀), 1 },
            { typeof(Model.闪), 2 },
            { typeof(Model.桃), 3 },
            { typeof(Model.顺手牵羊), 4 },
            { typeof(Model.过河拆桥), 5 },
            { typeof(Model.无中生有), 7 },
            { typeof(Model.决斗), 8 },
            { typeof(Model.南蛮入侵), 9 },
            { typeof(Model.万箭齐发), 10 },
            { typeof(Model.闪电), 11 },
            { typeof(Model.桃园结义), 12 },
            { typeof(Model.无懈可击), 13 },
            { typeof(Model.借刀杀人), 14 },
            { typeof(Model.乐不思蜀), 15 },
            // { typeof(Model.SubHorse), 18 },
            // { typeof(Model.PlusHorse), 18 },
            { typeof(Model.火杀), 16 },
            { typeof(Model.雷杀), 17 },
            { typeof(Model.酒), 82 },
            { typeof(Model.火攻), 83 },
            { typeof(Model.兵粮寸断), 84 },
            { typeof(Model.铁索连环), 85 },
        };

        private async void CardVoice(Model.Card card)
        {
            string url = Url.AUDIO + "spell/";

            if (!urls.ContainsKey(card.GetType()))
            {
                if (card is Model.Weapon || card is Model.Armor)
                {
                    url += "equipArmor.mp3";
                }
                else if (card is Model.SubHorse || card is Model.PlusHorse)
                {
                    url += "equipHorse.mp3";
                }
                else return;
            }
            else
            {
                int gender = card.Src.general.gender ? 1 : 2;
                url += "spell" + urls[card.GetType()] + "_" + gender + ".mp3";
            }

            effect.PlayOneShot(await WebRequest.GetClip(url));
        }

        private async void OnDamage(Model.UpdateHp model)
        {
            if (model is not Model.Damaged damaged) return;
            var type = damaged.type;
            string url = type == Model.Damaged.Type.Thunder ? "spell86_1" : type == Model.Damaged.Type.Fire ? "spell87_1" : "hurtSound";
            url = Url.AUDIO + "spell/" + url + ".mp3";
            effect.PlayOneShot(await WebRequest.GetClip(url));
        }

        private async void SkillVoice(Model.Skill model, List<Model.Player> dests)
        {
            var voice = model.src.currentSkin.voice.Find(x => x.name == model.name)?.url;
            if (voice is null) return;

            string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)] + ".mp3";
            effect.PlayOneShot(await WebRequest.GetClip(url));
        }

        private async void OnLock(Model.SetLock model)
        {
            string url = model.ByDamage ? "tiesuolianhuan_clear" : "tiesuolianhuan";
            url = Url.AUDIO + "spell/" + url + ".mp3";
            effect.PlayOneShot(await WebRequest.GetClip(url));
        }
    }
}