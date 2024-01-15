using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public Image nation;
    public Sprite[] nationSprite;
    private Dictionary<string, int> nationDict = new Dictionary<string, int>
        {
            { "蜀", 0 }, { "吴", 1 }, { "魏", 2 }, { "群", 3 }
        };
    public GameObject[] 阴阳鱼;
    public Text generalName;

    public GameObject skillPrefab;
    public Transform skillParent;

    public GameObject skinPrefab;
    public Transform skinParent;
    public ToggleGroup toggleGroup;

    public AudioSource effect;

    private Model.General model;
    private Model.Skin currentSkin;


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

    public async void Init(Model.General model)
    {
        this.model = model;

        // 基本信息
        nation.sprite = nationSprite[nationDict[model.nation]];
        for (int i = 0; i < 5; i++) 阴阳鱼[i].SetActive(i < model.hp_limit);
        generalName.text = model.name;

        // 技能
        for (int i = 0; i < model.skill.Count; i++)
        {
            var skill = Instantiate(skillPrefab, skillParent).GetComponent<SkillInfo>();
            skill.title.text = model.skill[i];
            skill.discribe.text = model.describe[i];
        }

        // 皮肤
        await InitSkin();
    }

    public async Task InitSkin()
    {
        var skins = (await Model.Skin.GetList(model.id));

        foreach (var i in skins) Instantiate(skinPrefab, skinParent).GetComponent<SkinBasic>().Init(i);

        // 切换到第一个皮肤 (经典形象)
        skinParent.GetChild(0).GetComponent<SkinBasic>().toggle.isOn = true;
    }

    public async void UpdateSkin(Model.Skin skin)
    {
        currentSkin = skin;
        Destroy(skeletonGraphicDaiji);
        Destroy(skeletonGraphicBj);

        daiji.SetActive(false);
        bj.SetActive(false);

        if (skin.dynamic)
        {
            Debug.Log(1);
            var ab = await ABManager.Instance.Load("dynamic/" + (skin.id + 200000));
            if (ab.Contains("daiji_SkeletonData.asset"))
            {
                image.gameObject.SetActive(false);
                await Util.WaitFrame();

                skeletonGraphicDaiji = daiji.AddComponent<SkeletonGraphic>();
                skeletonGraphicDaiji.skeletonDataAsset = ab.LoadAsset<SkeletonDataAsset>("daiji_SkeletonData.asset");
                skeletonGraphicDaiji.startingLoop = true;
                skeletonGraphicDaiji.startingAnimation = "play";
                skeletonGraphicDaiji.raycastTarget = false;
                daiji.SetActive(true);

                skeletonGraphicBj = bj.AddComponent<SkeletonGraphic>();
                skeletonGraphicBj.skeletonDataAsset = ab.LoadAsset<SkeletonDataAsset>("beijing_SkeletonData.asset");
                skeletonGraphicBj.startingLoop = true;
                skeletonGraphicBj.startingAnimation = "play";
                skeletonGraphicBj.raycastTarget = false;
                bj.SetActive(true);
                return;
            }
        }

        Debug.Log(2);
        string url = Url.GENERAL_IMAGE + "Big/" + skin.id + ".png";
        var texture = await WebRequest.GetTexture(url);
        if (texture is null) return;

        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        image.SetNativeSize();
        image.gameObject.SetActive(true);
    }

    public async void SkillVoice(string skillName)
    {
        var voice = currentSkin.voice.Find(x => x.name == skillName)?.url;
        if (voice is null) return;
        string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)];

        var clip = await WebRequest.GetClip(url);
        if (clip != null) effect.PlayOneShot(clip);
    }

    private void ClickBack()
    {
        gameObject.SetActive(false);
        GeneralList.Instance.gameObject.SetActive(true);
    }

    private async void ClickSetCharacter()
    {
        await WebRequest.GetWithToken(Url.DOMAIN_NAME + "changeCharacter?character=" + currentSkin.id);
    }
}