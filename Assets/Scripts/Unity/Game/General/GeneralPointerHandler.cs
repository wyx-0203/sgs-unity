using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private General general;

    private void Start()
    {
        general = GetComponent<General>();
    }

    /// <summary>
    /// 长按一秒，显示武将信息
    /// </summary>
    private void ShowInfo()
    {
        if (!TeammateHandCardPanel.Instance.gameObject.activeSelf) GeneralInfo.Instance.Show(general.model, general.skin.asset);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Invoke(nameof(ShowInfo), 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GeneralInfo.Instance.gameObject.activeSelf) GeneralInfo.Instance.gameObject.SetActive(false);
        else CancelInvoke(nameof(ShowInfo));
    }
}