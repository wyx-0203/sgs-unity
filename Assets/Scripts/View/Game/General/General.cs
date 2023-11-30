using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class General : MonoBehaviour
{
    // 武将信息

    public Skin skin;
    // public Image skin;
    // public GameObject dynamicSkin;
    // private SkeletonGraphic skeletonGraphic;
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
    private Color[] hpColor = new Color[] { Color.red, Color.yellow, Color.green };
    public Image yinYangYuSingle;

    public GameCore.General model { get; private set; }
    public GameCore.Skin skinModel { get; private set; }


    public async void Init(GameCore.General general)
    {
        model = general;

        name = general.name;
        generalName.text = general.name;
        nationBack.sprite = nationBackSprite[nationDict[general.nation]];
        nation.sprite = nationSprite[nationDict[general.nation]];

        SetHpLimit(general.hp_limit);

        skin.Set((await GameCore.Skin.GetList()).Find(x => x.general_id == general.id));
    }

    public void Init(GameCore.Player player)
    {
        model = player.general;

        generalName.text = player.general.name;
        nationBack.sprite = nationBackSprite[nationDict[player.general.nation]];
        nation.sprite = nationSprite[nationDict[player.general.nation]];

        SetHpLimit(player.hpLimit);
        SetHp(player.hp, player.hpLimit);

        if (player.currentSkin != null) skin.Set(player.currentSkin);
    }

    private static bool mutex;

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