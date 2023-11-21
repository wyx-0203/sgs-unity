using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillArea : SingletonMono<SkillArea>
{
    // 技能表
    private List<Skill> skills = new();
    // 已选技能
    private GameCore.Skill SelectedSkill => timer.temp.skill;
    private Player self => GameMain.Instance.self;
    private GameCore.Timer timer => GameCore.Timer.Instance;

    public Transform Long;
    public Transform Short;

    public GameObject 主动技;
    public GameObject 锁定技;
    public GameObject 限定技;

    private void Start()
    {
        GameCore.UpdateSkill.ActionView += OnUpdateSkill;
        GameCore.Timer.Instance.StopTimerView += Reset;
        GameCore.Main.Instance.MoveSeatView += MoveSeat;

        foreach (var i in GameCore.Main.Instance.AlivePlayers) InitOnePlayer(i);
    }

    private void OnDestroy()
    {
        GameCore.UpdateSkill.ActionView -= OnUpdateSkill;
        GameCore.Timer.Instance.StopTimerView -= Reset;
        GameCore.Main.Instance.MoveSeatView -= MoveSeat;
    }

    /// <summary>
    /// 更新技能时调用，例如关兴张苞获得或失去技能
    /// </summary>
    public void OnUpdateSkill(GameCore.UpdateSkill model)
    {
        InitOnePlayer(model.player);
    }

    public void InitOnePlayer(GameCore.Player player)
    {
        if (!player.isSelf) return;
        skills.RemoveAll(x => x.model.src == player);
        foreach (var i in Long.GetComponentsInChildren<Skill>()) if (i.model.src == player) Destroy(i.gameObject);

        var list = player.skills.ToList();
        foreach (var i in player.skills.Where(x => x.parent != null)) if (list.Find(x => x != i && x.parent == i.parent) != null) list.Remove(i);

        bool l = false;
        // 实例化预制件，添加到技能区
        foreach (var i in list)
        {

            var prefab = i is GameCore.Ultimate ? 限定技 : i.isObey ? 锁定技 : 主动技;
            var skill = Instantiate(prefab).GetComponent<Skill>();

            if (i.parent != null)
            {
                skill.gameObject.AddComponent<MultiSkill>().model = i.parent;
            }

            skill.gameObject.SetActive(player == self.model);
            skill.Init(i);

            if (list.Count % 2 == 1 && !l)
            {
                l = true;
                skill.transform.SetParent(Long, false);
                skill.transform.SetAsFirstSibling();
            }
            else skill.transform.SetParent(Short, false);

            skills.Add(skill);
        }
    }

    public void MoveSeat(GameCore.Player model)
    {
        foreach (var i in skills) i.gameObject.SetActive(i.model.src == model);
    }

    /// <summary>
    /// 开始操作时更新技能区
    /// </summary>
    public void OnStartPlay()
    {
        // var skills=this.skills.Where(x=>x.gameObject.activeSelf);
        // if (SelectedSkill != null)
        // {
        //     var skill = skills.Find(x => x.model == SelectedSkill);
        //     skill.toggle.interactable = true;
        //     if (SelectedSkill is GameCore.Triggered) skill.toggle.isOn = true;
        // }

        // else
        // {
        foreach (var i in skills)
        {
            i.GetComponent<MultiSkill>()?.OnStartPlay();
            if (i.model == SelectedSkill)
            {
                i.toggle.interactable = true;
                i.toggle.isOn = true;
                break;
            }
            i.toggle.interactable = (i.model is GameCore.Active || i.model is GameCore.Converted) && i.model.IsValid;
        }
        // }

    }

    /// <summary>
    /// 重置技能区
    /// </summary>
    public void Reset()
    {
        if (!timer.players.Contains(self.model)) return;
        foreach (var i in skills) i.Reset();
    }

    public void UnSelect()
    {
        skills.Find(x => x.toggle.isOn).toggle.isOn = false;
    }
}