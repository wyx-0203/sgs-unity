using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public Toggle toggle;
    public Text text;
    public GameObject effect;

    public GameCore.Skill model { get; set; }
    private bool refresh = true;

    public void Init(GameCore.Skill model)
    {
        this.model = model;
        name = model.name;
        text.text = model.name;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        effect.SetActive(value);
        if (model is not GameCore.Triggered)
        {
            GameCore.Timer.Instance.temp.skill = value ? model : null;
            if (refresh) OperationArea.Instance.OnClickSkill();
        }
    }

    /// <summary>
    /// 重置技能
    /// </summary>
    public void Reset()
    {
        toggle.interactable = false;
        refresh = false;
        toggle.isOn = false;
        refresh = true;
    }
}

public class MultiSkill : MonoBehaviour
{
    public GameCore.Skill.Multi model;

    public void OnStartPlay()
    {
        var skill = model.skills.Find(x => x is not GameCore.Triggered && x.IsValid);
        if (skill != null) GetComponent<Skill>().model = skill;
    }
}