using UnityEngine;
using UnityEngine.UI;

public class SelfPickSeat : MonoBehaviour
{
    public Image position;
    public Sprite[] posSprites;
    public Model.Player player { get; private set; }
    public GeneralBP general;

    public void Init(Model.Player player)
    {
        this.player = player;
        position.sprite = posSprites[player.turnOrder];
    }

}