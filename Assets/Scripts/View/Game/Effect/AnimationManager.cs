using Model;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : SingletonMono<AnimationManager>
{
    private void Start()
    {
        // GameCore.Card.UseCardView += UseCard;
        // GameCore.Skill.UseSkillView += UseSkill;
        EventSystem.Instance.AddEvent<UseCard>(OnUseCard);
        EventSystem.Instance.AddEvent<UseSkill>(OnUseSkill);
        EventSystem.Instance.AddEvent<Damage>(OnDamage);
    }

    private void OnDestroy()
    {
        // GameCore.Card.UseCardView -= UseCard;
        // GameCore.Skill.UseSkillView -= UseSkill;
        EventSystem.Instance.RemoveEvent<UseCard>(OnUseCard);
        EventSystem.Instance.RemoveEvent<UseSkill>(OnUseSkill);
        EventSystem.Instance.RemoveEvent<Damage>(OnDamage);
    }

    /// <summary>
    /// 武将位置
    /// </summary>
    private Vector3 Pos(int playerIndex) => GameMain.Instance.players[playerIndex].transform.position;

    private void OnUseCard(UseCard useCard)
    {
        Vector3 src = Pos(useCard.player);
        foreach (var i in useCard.dests)
        {
            if (i == useCard.player) continue;
            Vector3 dest = Pos(i);
            StartCoroutine(DrawLine(src, dest));
        }

        var card = DiscardPile.Instance.Cards.Find(x => x.id == useCard.id);
        if (card is null) return;

        string cardName = useCard.name;
        if (cardName == "杀" && Model.Card.Find(useCard.id).isRed) cardName = "红杀";
        var asset = GameAssets.Instance.cardEffect.Get(cardName);
        if (asset is null) return;

        var effect = new GameObject("Effect");
        effect.transform.SetParent(card.transform, false);
        effect.SetActive(false);

        var skeletonGraphic = effect.AddComponent<SkeletonGraphic>();
        skeletonGraphic.skeletonDataAsset = asset;
        skeletonGraphic.startingAnimation = "play";
        effect.SetActive(true);
    }

    // public Dictionary<string, string> map = new Dictionary<string, string>
    // {
    //     { "火杀", "SF_jichupai_eff_huosha_SkeletonData" },
    //     { "雷杀", "SF_jichupai_eff_leisha_SkeletonData" },
    //     { "杀", "SF_jichupai_eff_heisha_SkeletonData" },
    //     { "红杀", "SF_jichupai_eff_hongsha_SkeletonData" },
    //     { "闪", "SF_jichupai_eff_shan_SkeletonData" },
    //     { "桃", "SF_jichupai_eff_tao_SkeletonData" },
    //     { "酒", "SF_jichupai_eff_jiu_SkeletonData" },
    //     { "过河拆桥", "SF_kapai_eff_guohechaiqiao_SkeletonData" },
    //     { "火攻", "SF_kapai_eff_huogong_SkeletonData" },
    //     { "南蛮入侵", "SF_kapai_eff_nanmanruqin_SkeletonData" },
    //     { "桃园结义", "SF_kapai_eff_taoyuanjieyi_SkeletonData" },
    //     { "铁索连环", "SF_kapai_eff_tiesuolianhuan_SkeletonData" },
    //     { "万箭齐发", "SF_kapai_eff_wanjianqifa_SkeletonData" },
    //     { "无懈可击", "SF_kapai_eff_wuxiekeji_SkeletonData" },
    // };

    private void OnUseSkill(UseSkill useSkill)
    {
        Vector3 src = Pos(useSkill.player);
        foreach (var i in useSkill.dests)
        {
            if (i == useSkill.player) continue;
            Vector3 dest = Pos(i);
            StartCoroutine(DrawLine(src, dest));
        }
    }

    public GameObject linePrefab;

    private IEnumerator DrawLine(Vector3 src, Vector3 dest)
    {
        var line = Instantiate(linePrefab, transform).GetComponentInChildren<LineRenderer>();
        var arrow = line.transform.Find("Arrow");

        // 设置箭头的方向
        Vector3 dir = dest - src;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0f, 0f, angle);

        src = transform.InverseTransformPoint(src);
        dest = transform.InverseTransformPoint(dest);

        line.SetPosition(0, src);
        line.SetPosition(1, src);

        var speed = (dest - src).magnitude / 0.3f;

        while ((line.GetPosition(1)) != dest)
        {
            line.SetPosition(1, Vector3.MoveTowards(line.GetPosition(1), dest, speed * Time.deltaTime));
            arrow.position = transform.TransformPoint(line.GetPosition(1));
            // Debug.Log(line.GetPosition(1));
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        while ((line.GetPosition(0)) != dest)
        {
            line.SetPosition(0, Vector3.MoveTowards(line.GetPosition(0), dest, speed * Time.deltaTime));
            yield return null;
        }

        Destroy(line.gameObject);
    }

    // public AutoDestroyedEffect normalDamage;
    // public AutoDestroyedEffect thunderDamage;
    // public AutoDestroyedEffect fireDamage;

    private void OnDamage(Damage damage)
    {
        var type = damage.type;
        var prefab = type == Damage.Type.Thunder ? GameAssets.Instance.thunderDamageEffect
            : type == Damage.Type.Fire ? GameAssets.Instance.fireDamageEffect : GameAssets.Instance.normalDamageEffect;
        // var player = GameMain.Instance.players.Find(x => x.model == model.player);
        // var instance = 
        Instantiate(prefab, Player.Find(damage.player).transform);
        // var instance = Instantiate(prefab, Pos(model.player), new Quaternion(),transform);
        // url = Url.AUDIO + "spell/" + url + ".mp3";
        // effect.PlayOneShot(await WebRequest.GetClip(url));
    }
}