using UnityEngine;
using UnityEngine.UI;

namespace View
{
    /// <summary>
    /// 武将列表中的武将信息
    /// </summary>
    public class GeneralBasic : MonoBehaviour
    {
        public Button button;

        private Model.General model;

        public void Init(Model.General model)
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
}