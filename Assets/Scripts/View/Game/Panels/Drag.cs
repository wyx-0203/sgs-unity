using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IDragHandler
{
    private Canvas canvas;

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 移动拖拽框的位置
        transform.parent.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}