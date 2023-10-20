using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : SingletonMono<SkillArea>
    {
        // 技能表
        public List<Skill> Skills { get; private set; } = new List<Skill>();
        // 已选技能
        private Model.Skill SelectedSkill => timer.temp.skill;
        private Player self => SgsMain.Instance.self;
        private Model.Timer timer => Model.Timer.Instance;

        public Transform Long;
        public Transform Short;

        public GameObject 主动技;
        public GameObject 锁定技;
        public GameObject 限定技;

        private void Start()
        {
            Model.SgsMain.Instance.AfterBanPickView += InitSkillButton;
            Model.UpdateSkill.ActionView += InitSkillButton;
            Model.Timer.Instance.StopTimerView += Reset;

            Model.SgsMain.Instance.MoveSeatView += MoveSeat;
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.AfterBanPickView -= InitSkillButton;
            Model.UpdateSkill.ActionView -= InitSkillButton;
            Model.Timer.Instance.StopTimerView -= Reset;

            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;
        }

        /// <summary>
        /// 初始化技能区
        /// </summary>
        public void InitSkillButton()
        {
            Skills.Clear();
            foreach (Transform i in Long) if (i.name != "Short") Destroy(i.gameObject);
            foreach (Transform i in Short) Destroy(i.gameObject);

            foreach (var i in Model.SgsMain.Instance.AlivePlayers)
            {
                if (!i.isSelf) continue;
                int c = 0;
                // 实例化预制件，添加到技能区
                foreach (var j in i.skills)
                {
                    GameObject prefab;
                    // string str;
                    if (j is Model.Ultimate) prefab = 限定技;
                    else if (j.isObey) prefab = 锁定技;
                    else prefab = 主动技;

                    var instance = Instantiate(prefab).GetComponent<Skill>();
                    instance.name = j.Name;
                    instance.text.text = j.Name;
                    instance.model = j;

                    if (i.skills.Count % 2 == 1 && c == 0)
                    {
                        instance.transform.SetParent(Long, false);
                        instance.transform.SetAsFirstSibling();
                        c++;
                    }
                    else instance.transform.SetParent(Short, false);

                    Skills.Add(instance);
                }

                // MoveSeat(i);
            }
            MoveSeat(Model.SgsMain.Instance.AlivePlayers.Find(x => x.isSelf));
        }

        public void InitSkillButton(Model.UpdateSkill model)
        {
            // Debug.Log("a");
            if (model.player.team == self.model.team) InitSkillButton();
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in Skills) i.gameObject.SetActive(i.model.Src == model);
        }

        /// <summary>
        /// 显示进度条时更新技能区
        /// </summary>
        public void Init()
        {
            // if (timer.temp.skill != null) Skills.Find(x => x.model == timer.temp.skill)?.effect.SetActive(true);

            if (SelectedSkill != null)
            {
                var skill = Skills.Find(x => x.model == SelectedSkill);
                // if(skill is null) return;
                if (skill != null) skill.button.interactable = true;
                // if (SelectedSkill is Model.Converted) skill.Select();
                // foreach (var i in Skills)
                // {
                //     if (i.model != SelectedSkill) continue;
                //     if (i.model is not Model.Triggered) i.button.interactable = true;
                //     else i.effect.SetActive(true);
                // }
            }
            else
            {
                foreach (var i in Skills) i.button.interactable = i.model is not Model.Triggered && i.model.IsValid;
            }
        }

        /// <summary>
        /// 重置技能区
        /// </summary>
        public void Reset()
        {
            if (!timer.players.Contains(self.model)) return;
            // if (!timerTask.isWxkj && self.model != timerTask.players) return;

            foreach (var i in Skills) i.ResetSkill();
        }
    }
}
