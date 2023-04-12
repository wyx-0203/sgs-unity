using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Model
{
    public class Operation : Singleton<Operation>
    {
        public List<Card> Cards { get; private set; } = new List<Card>();
        public Card Converted { get; set; }
        public List<Player> Dests { get; private set; } = new List<Player>();
        public List<Card> Equips { get; private set; } = new List<Card>();
        public Skill skill { get; set; }

        public void Clear()
        {
            Cards.Clear();
            Converted = null;
            Dests.Clear();
            Equips.Clear();
            skill = null;
        }

        public bool AICommit()
        {
            if (Cards.Count > maxCard || Cards.Count < minCard || Dests.Count > MaxDest() || Dests.Count < MinDest())
            {
                // Debug.Log("dest.count=" + Dests.Count);
                // foreach (var i in Dests) Debug.Log("dest:" + i.PosStr);
                Clear();
                return false;
            }
            // Debug.Log("aicommit");

            Timer.Instance.Cards.AddRange(Cards);
            Timer.Instance.Dests.AddRange(Dests);
            Timer.Instance.Cards.AddRange(Equips);
            Timer.Instance.Skill = skill != null ? skill.Name : "";

            Clear();
            return true;
        }

        // public void AutoCommit()
        // {
        //     Dests.AddRange(SgsMain.Instance.AlivePlayers.Where(IsValidDest));
        //     AICommit();
        // }

        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public Func<int> MaxDest { get; set; }
        public Func<int> MinDest { get; set; }
        public Func<Card, bool> IsValidCard { get; set; }
        public Func<Player, bool> IsValidDest { get; set; }

        public void CopyTimer()
        {
            maxCard = Timer.Instance.maxCard;
            minCard = Timer.Instance.minCard;
            MaxDest = Timer.Instance.MaxDest;
            MinDest = Timer.Instance.MinDest;
            IsValidCard = Timer.Instance.IsValidCard;
            IsValidDest = Timer.Instance.IsValidDest;
        }
    }
}