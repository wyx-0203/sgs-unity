using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class ElsePlayerEquip : MonoBehaviour
    {
        public int Id { get; private set; }
        public Image cardImage;
        public Image suit;
        public Image weight;

        private Sprites sprites => Sprites.Instance;

        public void Init(Model.Equipage model)
        {
            Id = model.Id;
            name = model.Name;

            // var sprites = Sprites.Instance;
            cardImage.sprite = sprites.seat_equip[name];
            suit.sprite = sprites.seat_suit[model.Suit];
            if (model.Suit == "黑桃" || model.Suit == "草花") weight.sprite = sprites.seat_blackWeight[model.Weight];
            else weight.sprite = sprites.seat_redWeight[model.Weight];
        }
    }
}