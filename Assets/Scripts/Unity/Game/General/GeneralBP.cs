using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GeneralBP : MonoBehaviour
{
    public enum State
    {
        Self,
        Enemy,
        Ban,
        Selectable
    }

    private General general;
    public int id { get; private set; }

    public Button button;
    public GameObject ban;
    // 敌方已选
    public GameObject oppoPicked;

    public State state { get; private set; } = State.Selectable;

    private Transform pool => BanPick.Instance.pool;

    public void Init(int id)
    {
        this.id = id;

        general = GetComponent<General>();
        general.Init(Model.General.Get(id));

        button.onClick.AddListener(SetBpResult);
    }

    public void OnBan()
    {
        state = State.Ban;
        general.skin.SetColor(new Color(0.4f, 0.4f, 0.4f));
        ban.SetActive(true);
    }

    public void OnPick(bool isSelf)
    {
        if (!isSelf)
        {
            state = State.Enemy;
            general.skin.SetColor(new Color(0.4f, 0.4f, 0.4f));
            oppoPicked.SetActive(true);
        }
        else
        {
            state = State.Self;
            transform.SetParent(BanPick.Instance.selfPool);
        }
    }

    public void SetBpResult()
    {
        if (GeneralInfo.Instance.gameObject.activeSelf) return;

        button.interactable = false;
        BanPick.Instance.OnClickGeneral(id);
    }

    public async void ToSelfPick()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        button.interactable = true;
        transform.SetParent(pool);
        rectTransform = GetComponent<RectTransform>();

        await Util.WaitFrame();
        originPos = rectTransform.anchoredPosition;
    }

    private RectTransform rectTransform;
    private Vector2 originPos;
    private bool isSelect;

    public void OnClick()
    {
        if (GeneralInfo.Instance.gameObject.activeSelf) return;

        if (!isSelect)
        {
            var seat = BanPick.Instance.seats.FirstOrDefault(x => x.general is null);
            if (seat == null) return;

            transform.SetParent(seat.transform);
            seat.general = this;

            rectTransform.anchoredPosition = new Vector2(0, 7);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            isSelect = true;
        }
        else
        {
            var seat = transform.parent.GetComponent<SelfPickSeat>();
            seat.general = null;

            transform.SetParent(pool);

            rectTransform.anchoredPosition = originPos;
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            isSelect = false;
        }
        BanPick.Instance.UpdateCommitButton();
    }
}