using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武将列表中的武将信息
/// </summary>
public class GeneralBasic : MonoBehaviour
{
    public Button button;

    private GameCore.General model;

    public void Init(GameCore.General model)
    {
        this.model = model;
        name = model.name;
        GetComponent<General>().Init(model);

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GeneralList.Instance.ShowDetail(model);
    }
}