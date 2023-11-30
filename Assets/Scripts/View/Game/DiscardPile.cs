using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscardPile : SingletonMono<DiscardPile>
{
    public List<Card> Cards { get; } = new();

    private void Start()
    {
        GameCore.TurnSystem.Instance.FinishPlayView += Clear;
        GameCore.TurnSystem.Instance.FinishPhaseView += Clear;
    }

    private void OnDestroy()
    {
        GameCore.TurnSystem.Instance.FinishPlayView -= Clear;
        GameCore.TurnSystem.Instance.FinishPhaseView -= Clear;
    }

    private string GetUrl(GameCore.Card card)
    {
        switch (card)
        {
            case GameCore.火杀: return "SF_jichupai_eff_huosha_SkeletonData";
            case GameCore.雷杀: return "SF_jichupai_eff_leisha_SkeletonData";
            case GameCore.杀: return card.isRed ? "SF_jichupai_eff_hongsha_SkeletonData" : "SF_jichupai_eff_heisha_SkeletonData";
            case GameCore.闪: return "SF_jichupai_eff_shan_SkeletonData";
            case GameCore.桃: return "SF_jichupai_eff_tao_SkeletonData";
            case GameCore.酒: return "SF_jichupai_eff_jiu_SkeletonData";
            case GameCore.过河拆桥: return "SF_kapai_eff_guohechaiqiao_SkeletonData";
            case GameCore.火攻: return "SF_kapai_eff_huogong_SkeletonData";
            case GameCore.南蛮入侵: return "SF_kapai_eff_nanmanruqin_SkeletonData";
            case GameCore.桃园结义: return "SF_kapai_eff_taoyuanjieyi_SkeletonData";
            case GameCore.铁索连环: return "SF_kapai_eff_tiesuolianhuan_SkeletonData";
            case GameCore.万箭齐发: return "SF_kapai_eff_wanjianqifa_SkeletonData";
            case GameCore.无懈可击: return "SF_kapai_eff_wuxiekeji_SkeletonData";
            default: return null;
        }
    }

    public async void Add(Card card)
    {
        Cards.Add(card);
        card.SetParent(transform);

        if (!card.model.isUsing) return;
        string url = GetUrl(card.model);
        if (url is null) return;
        var asset = (await ABManager.Instance.Load("effect")).LoadAsset<SkeletonDataAsset>(url + ".asset");

        var effect = new GameObject("Effect");
        effect.transform.SetParent(card.transform, false);
        effect.SetActive(false);

        var skeletonGraphic = effect.AddComponent<SkeletonGraphic>();
        skeletonGraphic.skeletonDataAsset = asset;
        skeletonGraphic.startingAnimation = "play";
        effect.SetActive(true);
    }

    public async void Clear()
    {
        foreach (var i in Cards) Destroy(i.gameObject, 2);

        await new Delay(2.1f).Run();
        if (this == null) return;
        MoveAll(0.1f);
    }

    public HorizontalLayoutGroup horizontalLayoutGroup;

    /// <summary>
    /// 弃牌数量达到8时，更新间距
    /// </summary>
    private void UpdateSpacing()
    {
        if (transform.childCount >= 7)
        {
            var spacing = (810 - 121.5f * transform.childCount) / (float)(transform.childCount - 1);
            horizontalLayoutGroup.spacing = spacing;
        }
        else horizontalLayoutGroup.spacing = 0;
    }

    public async void MoveAll(float second)
    {
        UpdateSpacing();

        await Util.WaitFrame();
        foreach (var i in Cards) i.Move(second);
    }
}