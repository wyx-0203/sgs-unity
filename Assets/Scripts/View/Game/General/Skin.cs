using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skin : MonoBehaviour
{
    public Image static_;
    public GameObject dynamic;
    private SkeletonGraphic skeletonGraphic;
    public Material gray;

    public Model.Skin model { get; private set; }
    // private Player player;
    private Color color = Color.white;

    private void Start()
    {
        // player = GetComponentInParent<Player>();
    }

    public async void Set(Model.Skin skin)
    {
        model = skin;
        var player = GetComponentInParent<Player>();
        Destroy(skeletonGraphic);
        var material = player is null || player.model.alive ? null : gray;

        if (!skin.dynamic)
        {
            // 根据皮肤ID下载图片
            string url = Url.GENERAL_IMAGE + "Seat/" + skin.id + ".png";
            var texture = await WebRequest.GetTexture(url);
            if (texture is null) return;
            static_.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            static_.material = material;
            static_.color = color;
        }
        else
        {
            var ab = await ABManager.Instance.Load("dynamic/" + (skin.id + 200000));
            dynamic.SetActive(false);
            await Util.WaitFrame();

            skeletonGraphic = dynamic.AddComponent<SkeletonGraphic>();
            skeletonGraphic.skeletonDataAsset = ab.LoadAsset<SkeletonDataAsset>("daiji2_SkeletonData.asset");
            skeletonGraphic.startingLoop = true;
            skeletonGraphic.startingAnimation = "play";
            skeletonGraphic.raycastTarget = false;
            skeletonGraphic.material = material;
            skeletonGraphic.color = color;
        }

        static_.gameObject.SetActive(!skin.dynamic);
        dynamic.SetActive(skin.dynamic);

    }

    public void OnDead()
    {
        if (static_.gameObject.activeSelf) static_.material = gray;
        else skeletonGraphic.material = gray;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        if (static_.gameObject.activeSelf) static_.color = color;
        else skeletonGraphic.color = color;
    }

    // private void Update()
    // {
    //     if (skeletonGraphic != null) Debug.Log(skeletonGraphic.AnimationState);
    // }
}
