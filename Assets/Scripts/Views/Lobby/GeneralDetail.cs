using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace View
{
    public class GeneralDetail : SingletonMono<GeneralDetail>
    {
        // 武将大图
        public Image image;
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
        private List<Model.Skin> skins;

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
                skill.discribe.text = model.discribe[i];
            }

            // 皮肤
            await InitSkin();
        }

        public async Task InitSkin()
        {
            string url = Url.JSON + "skin/" + model.id.ToString().PadLeft(3, '0') + ".json";
            skins = JsonList<Model.Skin>.FromJson(await WebRequest.Get(url));

            foreach (var i in skins) Instantiate(skinPrefab, skinParent).GetComponent<SkinBasic>().Init(i);

            // 切换到第一个皮肤 (经典形象)
            skinParent.GetChild(0).GetComponent<SkinBasic>().toggle.isOn = true;
        }

        public async void UpdateSkin(Model.Skin skin)
        {
            currentSkin = skin;

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
            string url = Url.AUDIO + "skin/" + voice[Random.Range(0, voice.Count)] + ".mp3";

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
}