using UnityEngine;
using UnityEngine.UI;

public class PanelCard : MonoBehaviour
{
    // public GameCore.Card model { get; private set; }
    public int id { get; private set; }

    public void Init()
    {
        // model = GetComponent<Card>().model;
        id = GetComponent<Card>().id;
        var toggle = gameObject.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        toggle.interactable = true;
    }

    private void OnValueChanged(bool value)
    {
        CardPanel.Instance.OnClickCard(this);
    }
}