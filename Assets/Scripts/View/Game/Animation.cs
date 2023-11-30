using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Animation : SingletonMono<Animation>
{
    private void Start()
    {
        GameCore.Card.UseCardView += UseCard;
        GameCore.Skill.UseSkillView += UseSkill;
        GameCore.UpdateHp.ActionView += OnDamage;
    }

    private void OnDestroy()
    {
        GameCore.Card.UseCardView -= UseCard;
        GameCore.Skill.UseSkillView -= UseSkill;
        GameCore.UpdateHp.ActionView -= OnDamage;
    }

    /// <summary>
    /// 武将位置
    /// </summary>
    private Vector3 Pos(GameCore.Player model)
    {
        return GameMain.Instance.players.Find(x => x.model == model).transform.position;
    }

    private void UseCard(GameCore.Card card)
    {
        if (card.dests is null) return;

        Vector3 src = Pos(card.src);
        foreach (var i in card.dests)
        {
            if (i == card.src) continue;
            Vector3 dest = Pos(i);
            StartCoroutine(DrawLine(src, dest));
        }
    }

    private void UseSkill(GameCore.Skill skill, List<GameCore.Player> dests)
    {
        if (dests is null || skill is GameCore.Converted) return;

        Vector3 src = Pos(skill.src);
        foreach (var i in dests)
        {
            if (i == skill.src) continue;
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

    public SpineEffect normalDamage;
    public SpineEffect thunderDamage;
    public SpineEffect fireDamage;

    private void OnDamage(GameCore.UpdateHp model)
    {
        if (model is not GameCore.Damaged damaged) return;
        var type = damaged.type;
        var prefab = type == GameCore.Damaged.Type.Thunder ? thunderDamage : type == GameCore.Damaged.Type.Fire ? fireDamage : normalDamage;
        var player = GameMain.Instance.players.Find(x => x.model == model.player);
        var instance = Instantiate(prefab, player.transform);
        // var instance = Instantiate(prefab, Pos(model.player), new Quaternion(),transform);
        // url = Url.AUDIO + "spell/" + url + ".mp3";
        // effect.PlayOneShot(await WebRequest.GetClip(url));
    }
}