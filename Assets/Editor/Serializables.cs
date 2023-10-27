using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

// [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/General", order = 1)]
public class General : ScriptableObject
{
    // 编号
    public int id;
    // 势力
    public string nation;
    // 姓名
    public string _name;
    // 性别
    public bool gender;
    // 体力上限
    public int hp_limit;
    // 技能
    public List<Skill> skills = new();

    public void Init(Model.General model)
    {
        id = model.id;
        _name = model.name;
        nation = model.nation;
        gender = model.gender;
        hp_limit = model.hp_limit;
        for (int i = 0; i < model.skill.Count; i++)
        {
            var skill = ScriptableObject.CreateInstance<Skill>();
            skill._name = model.skill[i];
            skill.describe = model.describe[i];
            skill.src = this;
            skills.Add(skill);
        }
    }
}

public class Skill : ScriptableObject
{
    public string _name;
    public string describe;

    public General src;

    private static VisualTreeAsset template;

    public VisualElement NewElement()
    {
        if (template is null) template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/skill.uxml");
        var element = template.CloneTree();

        var skillName = element.Q<TextField>("skill-name");
        var skillDescribe = element.Q<TextField>("skill-describe");

        var serializedObject = new SerializedObject(this);
        skillName.BindProperty(serializedObject);
        skillDescribe.BindProperty(serializedObject);

        element.RegisterCallback((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("新建技能", x =>
            {
                var newSkill = ScriptableObject.CreateInstance<Skill>();
                newSkill.src = src;
                src.skills.Add(newSkill);
                element.parent.hierarchy.Add(newSkill.NewElement());
            });
            evt.menu.AppendAction("移除技能", x =>
            {
                src.skills.Remove(this);
                element.parent.hierarchy.Remove(element);
            });
        });

        return element;
    }
}