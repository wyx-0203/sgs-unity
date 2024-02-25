using UnityEngine;
using UnityEngine.UI;

public class GeneralList : SingletonMono<GeneralList>
{
    // 武将预制件
    public GameObject prefab;
    // 武将父对象
    public Transform generalParent;
    // 返回按钮
    public Button back;

    // 武将详情界面
    public GameObject detail;

    private void Start()
    {
        // 获得所有武将信息
        var list = Model.General.GetList();

        // 实例化每个武将
        foreach (var i in list)
        {
            var general = Instantiate(prefab, generalParent).GetComponent<GeneralBasic>();
            general.Init(i);
        }

        back.onClick.AddListener(ClickBack);
    }

    /// <summary>
    /// 进入详情界面
    /// </summary>
    public void ShowDetail(Model.General model)
    {
        gameObject.SetActive(false);
        detail.SetActive(true);
        GeneralDetail.Instance.Init(model);
    }

    private void ClickBack()
    {
        gameObject.SetActive(false);
        Lobby.Instance.ShowLobby();
    }
}