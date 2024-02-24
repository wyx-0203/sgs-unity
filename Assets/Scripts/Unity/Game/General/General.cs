using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class General : MonoBehaviour
{
    // 武将信息

    public Skin skin;
    public Text generalName;
    public Image nationBack;
    public Image nation;

    // 体力

    // 用阴阳鱼表示
    public GameObject imageGroup;
    public Image[] yinYangYu;

    // 用数字表示(超过5点体力)
    public GameObject numberGroup;
    public Text hp;
    public Text slash;
    public Text hpLimit;
    public Image yinYangYuSingle;

    public Model.General model { get; private set; }


    public void Init(Model.General model)
    {
        this.model = model;

        name = model.name;
        generalName.text = model.name;
        nationBack.sprite = GeneralsAsset.Instance.kindomBg.Get(model.kindom);
        nation.sprite = GeneralsAsset.Instance.kindom.Get(model.kindom);

        SetHpLimit(model.hpLimit);

        skin.Set(model.skins.First());
    }

    public void Init(Model.Player player)
    {
        model = player.general;

        generalName.text = player.general.name;
        nationBack.sprite = GeneralsAsset.Instance.kindomBg.Get(model.kindom);
        nation.sprite = GeneralsAsset.Instance.kindom.Get(model.kindom);

        SetHpLimit(player.hpLimit);
        SetHp(player.hp, player.hpLimit);

        skin.Set(player.currentSkin);
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
        int colorIndex = GeneralsAsset.Instance.GetBloodIndex(hp, hpLimit);

        if (hpLimit <= 5)
        {
            for (int i = 0; i < hpLimit; i++)
            {
                // 以损失体力设为黑色
                yinYangYu[i].sprite = GeneralsAsset.Instance.blood[hp > i ? colorIndex : 3];
            }
        }
        else
        {
            this.hp.text = Mathf.Max(hp, 0).ToString();

            this.hp.color = GeneralsAsset.Instance.bloodColor[colorIndex];
            slash.color = GeneralsAsset.Instance.bloodColor[colorIndex];
            this.hpLimit.color = GeneralsAsset.Instance.bloodColor[colorIndex];
            yinYangYuSingle.sprite = GeneralsAsset.Instance.blood[colorIndex];
        }
    }
}