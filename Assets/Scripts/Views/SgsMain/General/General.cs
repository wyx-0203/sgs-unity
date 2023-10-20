using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class General : MonoBehaviour
    {
        // 武将信息

        public Image skin;
        public Text generalName;
        private Dictionary<string, int> nationDict = new Dictionary<string, int>
        {
            { "蜀", 0 }, { "吴", 1 }, { "魏", 2 }, { "群", 3 }
        };
        public Image nationBack;
        public Sprite[] nationBackSprite;
        public Image nation;
        public Sprite[] nationSprite;

        // 体力

        // 用阴阳鱼表示
        public GameObject imageGroup;
        public Image[] yinYangYu;
        public Sprite[] 阴阳鱼Sprite;

        // 用数字表示(超过5点体力)
        public GameObject numberGroup;
        public Text hp;
        public Text slash;
        public Text hpLimit;
        private Color[] hpColor = new Color[]
        {
            Color.red, Color.yellow, Color.green
        };
        public Image yinYangYuSingle;

        public Model.General model { get; private set; }
        public string SkinId { get; private set; }
        public string SkinName { get; private set; }


        public void Init(Model.General model)
        {
            this.model = model;

            generalName.text = model.name;
            nationBack.sprite = nationBackSprite[nationDict[model.nation]];
            nation.sprite = nationSprite[nationDict[model.nation]];

            SetHpLimit(model.hp_limit);

            // 计算得到SkinId,例如 2 => 100201
            UpdateSkin(1 + model.id.ToString().PadLeft(3, '0') + "01", "经典形象");
        }

        public void Init(Model.Player model)
        {
            this.model = model.general;

            generalName.text = model.general.name;
            nationBack.sprite = nationBackSprite[nationDict[model.general.nation]];
            nation.sprite = nationSprite[nationDict[model.general.nation]];

            SetHpLimit(model.HpLimit);
            SetHp(model.Hp, model.HpLimit);

            UpdateSkin(model.currentSkin.id.ToString(), model.currentSkin.name);
        }

        /// <summary>
        /// 更新皮肤
        /// </summary>
        public async void UpdateSkin(string id, string name)
        {
            SkinId = id;
            SkinName = name;

            // 根据皮肤ID下载图片
            string url = Url.GENERAL_IMAGE + "Seat/" + SkinId + ".png";
            var texture = await WebRequest.GetTexture(url);
            if (texture is null) return;
            skin.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        /// <summary>
        /// 更新体力上限
        /// </summary>
        public void SetHpLimit(int hpLimit)
        {
            // 若体力上限<=5，用阴阳鱼表示
            if (hpLimit <= 5)
            {
                imageGroup.SetActive(true);
                numberGroup.SetActive(false);

                for (int i = 0; i < 5; i++) yinYangYu[i].gameObject.SetActive(i < hpLimit);
            }

            // 若体力上限>5，用数字表示
            else
            {
                imageGroup.SetActive(false);
                numberGroup.SetActive(true);

                this.hpLimit.text = hpLimit.ToString();
            }
        }

        /// <summary>
        /// 更新体力
        /// </summary>
        public void SetHp(int hp, int hpLimit)
        {
            // 阴阳鱼或数字颜色
            int colorIndex = GetColorIndex(hp, hpLimit);

            if (hpLimit <= 5)
            {
                for (int i = 0; i < hpLimit; i++)
                {
                    // 以损失体力设为黑色
                    yinYangYu[i].sprite = 阴阳鱼Sprite[hp > i ? colorIndex : 3];
                }
            }
            else
            {
                this.hp.text = Mathf.Max(hp, 0).ToString();

                this.hp.color = hpColor[colorIndex];
                slash.color = hpColor[colorIndex];
                this.hpLimit.color = hpColor[colorIndex];
                yinYangYuSingle.sprite = 阴阳鱼Sprite[colorIndex];
            }
        }

        public int GetColorIndex(int hp, int hpLimit)
        {
            var ratio = hp / (float)hpLimit;
            // 红
            if (ratio < 0.34) return 0;
            // 黄
            if (ratio < 0.67) return 1;
            // 绿
            return 2;
        }
    }
}