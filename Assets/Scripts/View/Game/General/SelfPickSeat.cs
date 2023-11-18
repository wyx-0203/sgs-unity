using UnityEngine;
using UnityEngine.UI;

public class SelfPickSeat : MonoBehaviour
{
    public Image position;
    public Sprite[] posSprites;
    private GameCore.Player player;
    public GeneralBP general;

    public void Init(GameCore.Player player)
    {
        this.player = player;
        position.sprite = posSprites[player.turnOrder];
    }

}