using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 制衡 : Active
    {
        public 制衡(Player src) : base(src) { }

        public override int MaxCard => int.MaxValue;
        public override int MinCard => 1;

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            int count = cards.Count;
            if (Src.HandCardCount > 0)
            {
                count++;
                foreach (var i in Src.HandCards)
                {
                    if (!cards.Contains(i))
                    {
                        count--;
                        break;
                    }
                }
            }

            await new Discard(Src, cards).Execute();
            await new GetCardFromPile(Src, count).Execute();
        }
    }
}