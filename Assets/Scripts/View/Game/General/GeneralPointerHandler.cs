using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private General general;
    private static GeneralInfo generalInfo;

    private void Start()
    {
        general = GetComponentInParent<General>();
    }

    /// <summary>
    /// 长按一秒，显示武将信息
    /// </summary>
    private void ShowInfo()
    {
        if (generalInfo is null)
        {
            generalInfo = GameMain.Instance.transform.Find("武将信息").GetComponent<GeneralInfo>();
        }
        generalInfo.Show(general.model, general.skinModel);
    }

#if UNITY_ANDROID

    public void OnPointerDown(PointerEventData eventData)
    {
        Invoke("ShowInfo", 1);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke("ShowInfo");
    }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke("ShowInfo");
    }

#else

        public void OnPointerDown(PointerEventData eventData)
        {
            CancelInvoke("ShowInfo");
        }

        public void OnPointerUp(PointerEventData eventData) { }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Invoke("ShowInfo", 1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (generalInfo != null && generalInfo.gameObject.activeSelf) generalInfo.gameObject.SetActive(false);
            else CancelInvoke("ShowInfo");
        }

#endif
}