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

    // 编号
    public int Id { get; private set; }

    public GameCore.Card model { get; private set; }

    public HandCard handCard { get; private set; }
    public PanelCard panelCard { get; private set; }

    private async void Init(GameCore.Card model, bool known)
    {
        Id = model.id;
        this.model = model;
        name = model.name;

        transform.SetParent(CardSystem.Instance.transform, false);

        target.name = model.name + "target";
        target.gameObject.AddComponent<Target>().Init(gameObject);

        if (!known) return;

        var sprites = Sprites.Instance;
        while (sprites.cardImage is null) await Task.Yield();

        // 初始化sprite

        image.sprite = sprites.cardImage[name];

        if (model.isConvert) return;

        suit.gameObject.SetActive(true);
        weight.gameObject.SetActive(true);
        suit.sprite = sprites.cardSuit[model.suit];
        if (model.suit == "黑桃" || model.suit == "草花") weight.sprite = sprites.blackWeight[model.weight];
        else weight.sprite = sprites.redWeight[model.weight];
    }

    public static Card New(GameCore.Card model, bool known)
    {
        var card = Instantiate(CardSystem.Instance.cardPrefab).GetComponent<Card>();
        card.Init(model, known);
        return card;
    }

    /// <summary>
    /// 初始化卡牌
    /// </summary>
    public static Card NewHandCard(GameCore.Card model)
    {
        var card = New(model, true);
        card.handCard = card.gameObject.AddComponent<HandCard>();
        card.handCard.Init();
        return card;
    }

    public static Card NewPanelCard(GameCore.Card model, bool known = true)
    {
        var card = New(model, known);
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
        if (DiscardArea.Instance.Cards.Contains(this)) DiscardArea.Instance.Cards.Remove(this);
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