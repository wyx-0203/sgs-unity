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

        public void Init(Model.Equipment model)
        {
            Id = model.id;
            name = model.name;

            // var sprites = Sprites.Instance;
            cardImage.sprite = sprites.seat_equip[name];
            suit.sprite = sprites.seat_suit[model.suit];
            if (model.suit == "黑桃" || model.suit == "草花") weight.sprite = sprites.seat_blackWeight[model.weight];
            else weight.sprite = sprites.seat_redWeight[model.weight];
        }
    }
}