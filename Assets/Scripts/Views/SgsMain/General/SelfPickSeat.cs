using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SelfPickSeat : MonoBehaviour
    {
        public Image position;
        public Sprite[] posSprites;
        private Model.Player player;
        public GeneralBP general;

        public void Init(Model.Player player)
        {
            this.player = player;
            position.sprite = posSprites[player.turnOrder];
        }

    }
}