using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralInfo : SingletonMono<GeneralInfo>
{
    public static new GeneralInfo Instance => SingletonMono<GeneralInfo>.Instance != null
        ? SingletonMono<GeneralInfo>.Instance
        : GameMain.Instance.transform.Find("武将信息").GetComponent<GeneralInfo>();

    public Image image;
    public Text generalName;
    public Transform skillParent;
    public GameObject skillPrefab;

    private List<SkillInfo> skillInfos = new();

    public async void Show(Model.General general, Model.Skin skin)
    {
        // generalName.text = "   " + skin.name + "*" + general.name;
        generalName.text = $"   {skin.name}*{general.name}";
        for (int i = 0; i < general.skill.Count; i++)
        {
            var skill = Instantiate(skillPrefab, skillParent).GetComponent<SkillInfo>();
            skill.title.text = general.skill[i];
            skill.discribe.text = general.describe[i];
            skillInfos.Add(skill);
        }

        // string url = Url.GENERAL_IMAGE + "Window/" + skin.id + ".png";
        string url = $"{Url.GENERAL_IMAGE}Window/{skin.id}.png";
        var texture = await WebRequest.GetTexture(url);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // foreach (Transform i in skillParent) if (i.name != "武将名" && i.name != "背景") Destroy(i.gameObject);
        foreach (var i in skillInfos) Destroy(i.gameObject);
        skillInfos.Clear();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
    }
}