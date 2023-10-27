using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : SingletonMono<SkillArea>
    {
        // 技能表
        private List<Skill> skills = new();
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
            // Model.SgsMain.Instance.AfterBanPickView += InitSkillButton;
            Model.UpdateSkill.ActionView += OnUpdateSkill;
            Model.Timer.Instance.StopTimerView += Reset;
            Model.SgsMain.Instance.MoveSeatView += MoveSeat;

            foreach (var i in Model.SgsMain.Instance.AlivePlayers) InitOnePlayer(i);
            MoveSeat(self.model);
        }

        private void OnDestroy()
        {
            // Model.SgsMain.Instance.AfterBanPickView -= InitSkillButton;
            Model.UpdateSkill.ActionView -= OnUpdateSkill;
            Model.Timer.Instance.StopTimerView -= Reset;
            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;
        }

        /// <summary>
        /// 初始化技能区
        /// </summary>
        // public void Init()
        // {
        //     skills.Clear();
        //     foreach (Transform i in Long) if (i.name != "Short") Destroy(i.gameObject);
        //     foreach (Transform i in Short) Destroy(i.gameObject);

        //     {
        //         if (!i.isSelf) continue;
        //         bool l = false;
        //         // 实例化预制件，添加到技能区
        //         foreach (var j in i.skills)
        //         {
        //             Debug.Log(j.Name);
        //             var prefab = j is Model.Ultimate ? 限定技 : j.isObey ? 锁定技 : 主动技;
        //             var skill = Instantiate(prefab).GetComponent<Skill>();
        //             skill.Init(j);

        //             if (i.skills.Count % 2 == 1 && !l)
        //             {
        //                 l = true;
        //                 skill.transform.SetParent(Long, false);
        //                 skill.transform.SetAsFirstSibling();
        //             }
        //             else skill.transform.SetParent(Short, false);

        //             skills.Add(skill);
        //         }

        //     }
        //     MoveSeat(self.model);
        // }

        /// <summary>
        /// 更新技能时调用，例如关兴张苞获得或失去技能
        /// </summary>
        public void OnUpdateSkill(Model.UpdateSkill model)
        {
            InitOnePlayer(model.player);
        }

        public void InitOnePlayer(Model.Player player)
        {
            if (!player.isSelf) return;
            skills.RemoveAll(x => x.model.src == player);
            foreach (var i in Long.GetComponentsInChildren<Skill>()) if (i.model.src == player) Destroy(i.gameObject);

            bool l = false;
            // 实例化预制件，添加到技能区
            foreach (var i in player.skills)
            {
                var prefab = i is Model.Ultimate ? 限定技 : i.isObey ? 锁定技 : 主动技;
                var skill = Instantiate(prefab).GetComponent<Skill>();
                skill.Init(i);

                if (player.skills.Count % 2 == 1 && !l)
                {
                    l = true;
                    skill.transform.SetParent(Long, false);
                    skill.transform.SetAsFirstSibling();
                }
                else skill.transform.SetParent(Short, false);

                skills.Add(skill);
            }
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in skills) i.gameObject.SetActive(i.model.src == model);
        }

        /// <summary>
        /// 开始操作时更新技能区
        /// </summary>
        public void OnStartPlay()
        {
            if (SelectedSkill != null)
            {
                var skill = skills.Find(x => x.model == SelectedSkill);
                if (skill != null) skill.button.interactable = true;
            }
            else
            {
                foreach (var i in skills) i.button.interactable = i.model is not Model.Triggered && i.model.IsValid;
            }
        }

        /// <summary>
        /// 重置技能区
        /// </summary>
        public void Reset()
        {
            if (!timer.players.Contains(self.model)) return;
            foreach (var i in skills) i.Reset();
        }

        public void ClickSkill(Model.Skill skill)
        {
            skills.Find(x => x.model == skill).OnClick();
        }
    }
}
