using UnityEngine;
using UnityEngine.UI;

public class Dest : MonoBehaviour
{
    public Toggle toggle;
    // 被选中边框
    public GameObject border;

    private Player player;
    public Model.Player model => player.model;

    // private bool refresh = true;
    // private ColorBlock colorBlock;

    private void Start()
    {
        player = GetComponent<Player>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        // colorBlock = toggle.colors;
    }

    private void OnValueChanged(bool value)
    {
        if (GeneralInfo.Instance != null && GeneralInfo.Instance.gameObject.activeSelf) return;

        border.SetActive(value);
        DestArea.Instance.OnClickDest(model.index, value);
        // if (value) GameCore.Timer.Instance.temp.dests.Add(model);
        // else GameCore.Timer.Instance.temp.dests.Remove(model);

        // if (refresh)
        // {
        //     DestArea.Instance.Update_();
        //     OperationArea.Instance.UpdateButtonArea();
        // }
    }

    /// <summary>
    /// 取消选中
    /// </summary>
    // public void Unselect()
    // {
    //     refresh = false;
    //     toggle.isOn = false;
    //     refresh = true;
    // }

    /// <summary>
    /// 设置阴影
    /// </summary>
    // public void SetShadow()
    // {
    //     player.general.skin.SetColor(toggle.interactable ? Color.white : new(0.6f, 0.6f, 0.6f));
    //     // colorBlock.disabledColor = ;
    //     // toggle.colors = colorBlock;
    // }

    public void SetInteractable(bool value)
    {
        toggle.interactable = value;
        player.general.skin.SetColor(value ? Color.white : new(0.6f, 0.6f, 0.6f));
    }

    /// <summary>
    /// 重置玩家按键
    /// </summary>
    public void Reset()
    {
        toggle.interactable = false;
        player.general.skin.SetColor(Color.white);
        // SetInteractable(false);
        // colorBlock.disabledColor = Color.white;
        // toggle.colors = colorBlock;
        // Unselect();
        toggle.isOn = false;
    }
}