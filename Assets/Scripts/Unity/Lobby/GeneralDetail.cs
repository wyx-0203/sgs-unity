using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GeneralDetail : SingletonMono<GeneralDetail>
{
    // 武将大图
    public Image image;
    public GameObject daiji;
    private SkeletonGraphic skeletonGraphicDaiji;
    public GameObject bj;
    private SkeletonGraphic skeletonGraphicBj;
    // 设为形象
    public Button SetCharacter;
    // 返回
    public Button back;

    public Image kindom;

    public GameObject[] 阴阳鱼;
    public Text generalName;

    public GameObject skillPrefab;
    public Transform skillParent;

    public GameObject skinPrefab;
    public Transform skinParent;
    public ToggleGroup toggleGroup;

    public AudioSource effect;

    private Model.General model;
    private SkinAsset currentSkin;


    private void Start()
    {
        back.onClick.AddListener(ClickBack);
        SetCharacter.onClick.AddListener(ClickSetCharacter);
    }

    private void OnDisable()
    {
        image.gameObject.SetActive(false);
        foreach (Transform i in skillParent) Destroy(i.gameObject);
        foreach (Transform i in skinParent) Destroy(i.gameObject);
    }

    public void Init(Model.General model)
    {
        this.model = model;

        // 基本信息
        kindom.sprite = GeneralsAsset.Instance.kindom.Get(model.kindom);
        for (int i = 0; i < 5; i++) 阴阳鱼[i].SetActive(i < model.hpLimit);
        generalName.text = model.name;

        // 技能
        foreach (var i in model.skills)
        {
            var skill = Instantiate(skillPrefab, skillParent).GetComponent<SkillInfo>();
            skill.title.text = i;
            skill.discribe.text = SkillAsset.Get(i).describe;
        }

        // 皮肤
        InitSkin();
    }

    public void InitSkin()
    {
        foreach (var i in model.skins) Instantiate(skinPrefab, skinParent).GetComponent<SkinBasic>().Init(i);

        // 切换到第一个皮肤 (经典形象)
        skinParent.GetChild(0).GetComponent<SkinBasic>().toggle.isOn = true;
    }

    public async void UpdateSkin(SkinAsset skin)
    {
        currentSkin = skin;
        Destroy(skeletonGraphicDaiji);
        Destroy(skeletonGraphicBj);

        daiji.SetActive(false);
        bj.SetActive(false);

        if (skin.dynamic)
        {
            try
            {
                image.gameObject.SetActive(false);

                skeletonGraphicDaiji = daiji.AddComponent<SkeletonGraphic>();
                skeletonGraphicDaiji.skeletonDataAsset = await skin.GetDaijiSkeleton();
                skeletonGraphicDaiji.startingLoop = true;
                skeletonGraphicDaiji.startingAnimation = "play";
                skeletonGraphicDaiji.raycastTarget = false;
                daiji.SetActive(true);

                skeletonGraphicBj = bj.AddComponent<SkeletonGraphic>();
                skeletonGraphicBj.skeletonDataAsset = await skin.GetBgSkeleton();
                skeletonGraphicBj.startingLoop = true;
                skeletonGraphicBj.startingAnimation = "play";
                skeletonGraphicBj.raycastTarget = false;
                bj.SetActive(true);
                return;
            }
            catch (Exception e) { Debug.Log(e); }
        }

        image.sprite = await skin.GetBigImage();
        image.SetNativeSize();
        image.gameObject.SetActive(true);
    }

    public async void SkillVoice(string skillName)
    {
        var clip = await currentSkin.GetVoice(skillName);
        if (clip != null) effect.PlayOneShot(clip);
    }

    private void ClickBack()
    {
        gameObject.SetActive(false);
        GeneralList.Instance.gameObject.SetActive(true);
    }

    private async void ClickSetCharacter()
    {
        await WebRequest.GetWithToken($"{Url.DOMAIN_NAME}changeCharacter?character={currentSkin.id}");
    }
}