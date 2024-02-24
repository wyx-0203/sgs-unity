using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    // 卡牌图片
    public Image image;
    // 花色
    public Image suit;
    // 点数
    public Image weight;
    // 目标 (用于实现过渡效果)
    public Transform target;
    public GameObject effect;

    // 编号
    public int id => model.id;
    // public string convert { get; private set; }
    public string convert { get; private set; }
    public Model.Card model { get; private set; }

    // public HandCard handCard { get; private set; }
    public PanelCard panelCard { get; private set; }

    // public static Model.Card Find(int id) => GameMain.Instance.cardModels[id];
    // private static Model.Card[] cards;

    private void Init(int id, bool display)
    {
        model = Model.Card.Find(id);
        name = model.name;

        transform.SetParent(CardManager.Instance.transform, false);

        target.name = name + "Target";
        target.gameObject.AddComponent<Target>().Init(gameObject);

        if (!display) return;

        image.sprite = GameAsset.Instance.cardImage.Get(name);

        if (model.isVirtual) return;

        suit.gameObject.SetActive(true);
        weight.gameObject.SetActive(true);
        suit.sprite = GameAsset.Instance.cardSuit.Get(model.suit);
        if (model.isBlack) weight.sprite = GameAsset.Instance.cardBlackWeight[model.weight];
        else weight.sprite = GameAsset.Instance.cardRedWeight[model.weight];
    }

    public static Card New(int id, bool known)
    {
        var card = Instantiate(GameAsset.Instance.card);
        card.Init(id, known);
        return card;
    }

    /// <summary>
    /// 初始化卡牌
    /// </summary>
    public static Card NewHandCard(int id)
    {
        var card = New(id, true);
        // card.handCard = 
        card.gameObject.AddComponent<HandCard>();
        // handCard.Init();
        return card;
    }

    // public static Card NewVirtualCard(string name)
    // {
    //     var card = Instantiate(CardSystem.Instance.cardPrefab).GetComponent<Card>();
    //     card.Init(name);
    //     // card.handCard = 
    //     card.gameObject.AddComponent<HandCard>();
    //     // card.handCard.Init();
    //     return card;
    // }

    public static Card NewPanelCard(int id, bool known = true)
    {
        var card = New(id, known);
        card.panelCard = card.gameObject.AddComponent<PanelCard>();
        card.panelCard.Init();
        return card;
    }

    public void SetParent(Transform parent)
    {
        target.SetParent(parent, false);
    }

    private bool isMoving;

    /// <summary>
    /// 实体向目标方向移动
    /// </summary>
    public void Move(float second)
    {
        if (isMoving || transform.position == target.position) return;

        if (second == 0 || !gameObject.activeSelf)
        {
            transform.position = target.position;
            return;
        }

        isMoving = true;
        StartCoroutine(MoveAsync(second));
    }

    private IEnumerator MoveAsync(float second)
    {
        while (target != null && transform.position != target.position)
        {
            var dx = (target.position - transform.position).magnitude / second * Time.deltaTime;
            second -= Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, dx);
            if (second <= 0) transform.position = target.position;
            yield return null;
        }
        isMoving = false;
    }

    private void OnEnable()
    {
        target.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        transform.position = target.position;
        target.gameObject.SetActive(false);
        isMoving = false;
    }

    private void OnDestroy()
    {
        if (target != null) Destroy(target.gameObject);
        if (DiscardPile.Instance.Cards.Contains(this)) DiscardPile.Instance.Cards.Remove(this);
    }
}

class Target : MonoBehaviour
{
    private GameObject instance;
    public void Init(GameObject instance)
    {
        this.instance = instance;
    }
    private void OnDestroy()
    {
        if (instance != null) Destroy(instance);
    }
}