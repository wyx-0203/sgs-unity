using UnityEngine;
using UnityEngine.UI;

public class SkinBasic : MonoBehaviour
{
    public Text title;
    public Image image;
    public Toggle toggle;
    public GameObject isSelect;

    private SkinAsset asset;

    public async void Init(int id)
    {
        asset = SkinAsset.Get(id);
        toggle.onValueChanged.AddListener(OnValueChanged);
        toggle.group = GeneralDetail.Instance.toggleGroup;
        title.text = asset.name;
        image.sprite = await asset.GetSeatImage();
    }

    private void OnValueChanged(bool value)
    {
        if (value)
        {
            isSelect.SetActive(true);
            GeneralDetail.Instance.UpdateSkin(asset);
        }
        else isSelect.SetActive(false);
    }
}