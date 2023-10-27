using System;
using System.Collections.Generic;
using System.Linq;
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

        public Message ToMessage() => new Message(this);

        public override string ToString()
        {
            string str = "action=" + action;
            if (!action) return str + '\n';

            str += "\ncards=" + string.Join(' ', cards);
            str += "\ndests=" + string.Join(' ', dests);
            str += "\nskill=" + skill?.name;
            str += "\nsrc=" + src;
            return str + '\n';
        }


        [Serializable]
        public class Message : WebsocketMessage
        {
            public bool action;
            public List<int> cards;
            public List<int> dests;
            public string skill;
            public int src;
            public string other;

            public Message(Decision decision)
            {
                action = decision.action;
                cards = decision.cards.Select(x => x.id).ToList();
                dests = decision.dests.Select(x => x.position).ToList();
                skill = decision.skill?.name;
                other = decision.converted?.name;
                src = decision.src is Player p ? p.position : 0;
                msg_type = "set_result";
            }

            /// <summary>
            /// 投降消息
            /// </summary>
            public Message(Team team)
            {
                other = team == Team.BLUE ? "blue" : "red";
                msg_type = "surrender";
            }

            public Decision ToDecision() => new Decision
            {
                action = action,
                cards = cards.Select(x => CardPile.Instance.cards[x]).ToList(),
                dests = dests.Select(x => SgsMain.Instance.players[x]).ToList(),
                skill = Timer.Instance.players.FirstOrDefault()?.FindSkill(skill),
                converted = Timer.Instance.multiConvert.Find(x => x.name == other),
                src = SgsMain.Instance.players[src],
            };

            public override string ToString() => ToDecision().ToString();
        }

        public class List : Singleton<List>
        {
            private List<Message> list = new();
            private int index;

            public async Task<Decision> Pop()
            {
                // UnityEngine.Debug.Log(111);
                while (index == list.Count) await Task.Yield();
                var message = list[index++];
                if (message.msg_type == "surrender") throw new GameOverException(message.other == "blue" ? Team.BLUE : Team.RED);
                return message.ToDecision();
            }

            public void Push(Decision decision) => list.Add(decision.ToMessage());
            public void Push(Message message) => list.Add(message);
            public bool IsEmpty => index == list.Count;

            public void AddRange(List listInstance) => list.AddRange(listInstance.list);

            public override string ToString() => "index=" + index + "\ncount=" + list.Count + '\n' + string.Join("\n", list);
        }
    }
}