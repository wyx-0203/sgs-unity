using Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillArea : SingletonMono<SkillArea>
{
    // 技能表
    private List<Skill> skills = new();

    private Player self => GameMain.Instance.firstPerson;

    private SinglePlayQuery current
    {
        get => PlayArea.Instance.current;
        set => PlayArea.Instance.current = value;
    }

    private PlayQuery playQuery => PlayArea.Instance.playQuery;

    private PlayDecision decision => PlayArea.Instance.decision;

    public Transform Long;
    public Transform Short;

    public GameObject 主动技;
    public GameObject 锁定技;
    public GameObject 限定技;

    protected override void Awake()
    {
        base.Awake();

        EventSystem.Instance.AddEvent<InitGeneral>(InitOnePlayer);
        EventSystem.Instance.AddEvent<UpdateSkill>(InitOnePlayer);
        EventSystem.Instance.AddEvent<FinishPlay>(Reset);

        GameMain.Instance.OnChangeView += OnChangeView;
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<InitGeneral>(InitOnePlayer);
        EventSystem.Instance.RemoveEvent<UpdateSkill>(InitOnePlayer);
        EventSystem.Instance.RemoveEvent<FinishPlay>(Reset);
    }

    private void InitOnePlayer(InitGeneral initGeneral)
    {
        InitOnePlayer(initGeneral.skills, Player.Find(initGeneral.player).model);
    }

    /// <summary>
    /// 更新技能时调用，例如关兴张苞获得或失去技能
    /// </summary>
    public void InitOnePlayer(UpdateSkill updateSkill)
    {
        InitOnePlayer(updateSkill.skills, Player.Find(updateSkill.player).model);
    }

    public void InitOnePlayer(List<Model.Skill> skills, Model.Player src)
    {
        if (!src.isSelf) return;

        this.skills.RemoveAll(x => x.src == src);
        foreach (var i in Long.GetComponentsInChildren<Skill>().Where(x => x.src == src)) Destroy(i.gameObject);

        bool l = false;
        // 实例化预制件，添加到技能区
        foreach (var i in skills)
        {
            GameObject prefab;
            switch (i.type)
            {
                case Model.Skill.Type.Limited: prefab = 限定技; break;
                case Model.Skill.Type.Passive: prefab = 锁定技; break;
                default: prefab = 主动技; break;
            }
            var skill = Instantiate(prefab).GetComponent<Skill>();

            skill.gameObject.SetActive(src == self.model);
            skill.Init(i, src);

            if (skills.Count % 2 == 1 && !l)
            {
                l = true;
                skill.transform.SetParent(Long, false);
                skill.transform.SetAsFirstSibling();
            }
            else skill.transform.SetParent(Short, false);

            this.skills.Add(skill);
        }
    }

    public void OnChangeView(Model.Player player)
    {
        foreach (var i in skills) i.gameObject.SetActive(i.src == player);
    }

    /// <summary>
    /// 开始操作时更新技能区
    /// </summary>
    public void OnStartPlay()
    {
        // 存在已选技能
        var skill = skills.Find(x => x.gameObject.activeSelf && x.name == current.skillName);
        if (skill != null)
        {
            skill.toggle.interactable = true;
            skill.toggle.isOn = true;
        }

        // 设置可发动的技能
        else foreach (var i in skills) i.toggle.interactable = !playQuery.skills.All(x => x.skillName != i.name);
    }

    private bool inReset;

    /// <summary>
    /// 重置技能区
    /// </summary>
    private void Reset(FinishPlay finishPlay)
    {
        if (Player.IsSelf(finishPlay.player))
        {
            inReset = true;
            foreach (var i in skills) i.Reset();
            inReset = false;
        }
    }

    public void OnClickSkill(string skill, bool value)
    {
        if (skill == playQuery.origin.skillName) return;
        if (inReset) return;

        CardArea.Instance.Reset();
        EquipArea.Instance.ResetBySkill(skill);
        VirtualCardArea.Instance.Reset();
        DestArea.Instance.Reset();

        if (value)
        {
            decision.skill = skill;
            current = playQuery.skills.Find(x => x.skillName == skill);
        }
        else
        {
            decision.skill = "";
            current = playQuery.origin;
        }

        OnStartPlay();
        CardArea.Instance.OnStartPlay();
        EquipArea.Instance.OnStartPlay();
        VirtualCardArea.Instance.OnStartPlay();
        DestArea.Instance.OnStartPlay();
        PlayArea.Instance.UpdateButtonArea();
    }

    public void UnSelect()
    {
        var skill = skills.Find(x => x.name == current.skillName);
        if (skill != null) skill.toggle.isOn = false;
        else EquipArea.Instance.equipments.Values.First(x => x.name == current.skillName).toggle.isOn = false;
    }
}