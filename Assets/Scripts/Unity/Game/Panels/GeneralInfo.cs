using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeneralInfo : SingletonMono<GeneralInfo>
{
    public static new GeneralInfo Instance => SingletonMono<GeneralInfo>.Instance != null
        ? SingletonMono<GeneralInfo>.Instance
        : Game.Instance.transform.Find("武将信息").GetComponent<GeneralInfo>();

    public Image image;
    public Text generalName;
    public Transform skillParent;
    public GameObject skillPrefab;

    private readonly List<SkillInfo> skillInfos = new();

    public async void Show(Model.General general, SkinAsset skin)
    {
        generalName.text = $"   {skin.name}*{general.name}";
        skillInfos.AddRange(general.skills.Select(x =>
        {
            var skill = Instantiate(skillPrefab, skillParent).GetComponent<SkillInfo>();
            skill.title.text = x;
            skill.discribe.text = SkillAsset.Get(x).describe;
            return skill;
        }));

        image.sprite = await skin.GetWindowImage();

        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        foreach (var i in skillInfos) Destroy(i.gameObject);
        skillInfos.Clear();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
    }
}