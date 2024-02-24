using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public Toggle toggle;
    public Text text;
    public GameObject effect;

    public Model.Player src { get; private set; }

    public void Init(string name, Model.Player src)
    {
        this.src = src;
        this.name = name;
        text.text = name;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        effect.SetActive(value);
        SkillArea.Instance.OnClickSkill(name, value);
    }

    /// <summary>
    /// 重置技能
    /// </summary>
    public void Reset()
    {
        toggle.interactable = false;
        toggle.isOn = false;
    }
}