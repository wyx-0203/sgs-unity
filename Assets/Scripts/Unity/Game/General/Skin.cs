using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class Skin : MonoBehaviour
{
    public Image static_;
    public GameObject dynamic;
    private SkeletonGraphic skeletonGraphic;
    public Material gray;

    public SkinAsset asset { get; private set; }
    private Color color = Color.white;

    public async void Set(int id)
    {
        asset = SkinAsset.Get(id);
        var player = GetComponentInParent<Player>();
        Destroy(skeletonGraphic);
        var material = player == null || player.model.alive ? null : gray;

        if (!asset.dynamic)
        {
            // 根据皮肤ID下载图片
            static_.sprite = await asset.GetSeatImage();
            static_.material = material;
            static_.color = color;
        }
        else
        {
            dynamic.SetActive(false);
            await Util.WaitFrame();

            skeletonGraphic = dynamic.AddComponent<SkeletonGraphic>();
            skeletonGraphic.skeletonDataAsset = await asset.GetGameSkeleton();
            skeletonGraphic.startingLoop = true;
            skeletonGraphic.startingAnimation = "play";
            skeletonGraphic.raycastTarget = false;
            skeletonGraphic.material = material;
            skeletonGraphic.color = color;
        }

        static_.gameObject.SetActive(!asset.dynamic);
        dynamic.SetActive(asset.dynamic);
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
}
