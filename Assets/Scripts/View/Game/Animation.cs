using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation : SingletonMono<Animation>
{
    private void Start()
    {
        GameCore.Card.UseCardView += UseCard;
        GameCore.Skill.UseSkillView += UseSkill;
    }

    private void OnDestroy()
    {
        GameCore.Card.UseCardView -= UseCard;
        GameCore.Skill.UseSkillView -= UseSkill;
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
        var line = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();

        src = transform.InverseTransformPoint(src);
        dest = transform.InverseTransformPoint(dest);

        line.SetPosition(0, src);
        line.SetPosition(1, src);

        var speed = (dest - src).magnitude / 0.3f;

        while ((line.GetPosition(1)) != dest)
        {
            line.SetPosition(1, Vector3.MoveTowards(line.GetPosition(1), dest, speed * Time.deltaTime));
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
}