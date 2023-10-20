using System.Collections;
using UnityEngine;

namespace View
{
    public class Animation : SingletonMono<Animation>
    {
        private void Start()
        {
            Model.Card.UseCardView += UseCard;
            Model.Skill.UseSkillView += UseSkill;
        }

        private void OnDestroy()
        {
            Model.Card.UseCardView -= UseCard;
            Model.Skill.UseSkillView -= UseSkill;
        }

        /// <summary>
        /// 武将位置
        /// </summary>
        private Vector3 Pos(Model.Player model)
        {
            // if (model == SgsMain.Instance.self.model) return Self.Instance.transform.position;
            // else 
            return SgsMain.Instance.players.Find(x => x.model == model).transform.position;
        }

        private void UseCard(Model.Card card)
        {
            if (card.Dests is null) return;

            Vector3 src = Pos(card.Src);
            foreach (var i in card.Dests)
            {
                if (i == card.Src) continue;
                Vector3 dest = Pos(i);
                StartCoroutine(DrawLine(src, dest));
            }
        }

        private void UseSkill(Model.Skill skill)
        {
            if (skill.Dests is null || skill is Model.Converted) return;

            Vector3 src = Pos(skill.Src);
            foreach (var i in skill.Dests)
            {
                if (i == skill.Src) continue;
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
}