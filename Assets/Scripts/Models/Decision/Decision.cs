using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public class Decision
    {
        public bool action = false;
        public List<Card> cards = new();
        public List<Player> dests = new();
        public Skill skill;
        public Card converted;
        public Player src;

        public static List<Decision> list = new();
        public static int index;

        // public void Add(Decision decision) => _list.Add(decision);
        public static async Task<Decision> Pop()
        {
            while (index == list.Count) await Task.Yield();
            return list[index++];
        }
    }
}