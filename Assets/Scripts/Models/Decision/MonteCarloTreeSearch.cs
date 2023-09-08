// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;

// namespace Model
// {
//     public class MonteCarloTreeSearch : Singleton<MonteCarloTreeSearch>
//     {
//         public async void Run()
//         {
//             SgsMain.RemoveInstance();
//             CardPile.RemoveInstance();
//             TurnSystem.RemoveInstance();
//             Timer.RemoveInstance();
//             WxkjTimer.RemoveInstance();
//             CompeteTimer.RemoveInstance();
//             CardPanel.RemoveInstance();
//             Decision.list = new List<Decision>(Decision.list);
//             Decision.index = 0;

//             SgsMain.Instance.Run();
//             while (Decision.index < decisions.Count) await Task.Yield();

//             var player = Timer.Instance.players[0];
//             cards = player.HandCards.Union(player.Equipments.Values).Where(x => x != null && Timer.Instance.isValidCard(x)).ToList();
//             int minCard = Timer.Instance.minCard;
//             int maxCard = Mathf.Min(Timer.Instance.maxCard, cards.Count);
//             temp = Timer.Instance.temp;
//             for (int i = minCard; i <= maxCard; i++) TraverseCards(i);
//         }

//         private void TraverseCards(int length, int index = 0)
//         {
//             if (length == 0)
//             {
//                 dests = SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).ToList();
//                 int minDest = Timer.Instance.minDest();
//                 int maxDest = Mathf.Min(Timer.Instance.maxDest(), dests.Count);
//                 // if (minDest > maxDest) return;
//                 if (minDest == 2 && maxDest == 2) Traverse2Dests();
//                 else for (int i = minDest; i <= maxDest; i++) TraverseDests(i);
//                 return;
//             }

//             for (int i = index; i < cards.Count; i++)
//             {
//                 if (!Timer.Instance.isValidCard(cards[i])) continue;

//                 temp.cards.Add(cards[i]);
//                 TraverseCards(length - 1, index + 1);
//                 temp.cards.RemoveAt(temp.cards.Count - 1);
//             }
//         }

//         private List<Card> cards;
//         private List<Player> dests;
//         private Decision temp;
//         // private List<Card> subsequence = new();

//         private void TraverseDests(int length, int index = 0)
//         {
//             if (length == 0)
//             {
//                 // 
//                 return;
//             }
//             // while(temp.dests.Count)
//             // var dests = 
//             for (int i = index; i < dests.Count; i++)
//             {
//                 temp.dests.Add(dests[i]);
//                 TraverseDests(length - 1, index + 1);
//                 temp.dests.RemoveAt(temp.dests.Count - 1);
//             }
//         }

//         private void Traverse2Dests()
//         {
//             // var firstDests = SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).ToList();
//             foreach (var i in dests)
//             {
//                 temp.dests.Add(i);
//                 var secondDests = SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x) && x != i);

//                 foreach (var j in secondDests)
//                 {
//                     temp.dests.Add(j);
//                     // 
//                     temp.dests.Remove(j);
//                 }
//                 temp.dests.Remove(i);
//             }
//         }

//         public MonteCarloTreeSearch()
//         {
//             _sgsMain = SgsMain.Instance;
//             _cardPile = CardPile.Instance;
//             _turnSystem = TurnSystem.Instance;
//             _timer = Timer.Instance;
//             _wxkjTimer = WxkjTimer.Instance;
//             _competeTimer = CompeteTimer.Instance;
//             _cardPanel = CardPanel.Instance;
//             decisions = Decision.list;
//         }

//         private SgsMain _sgsMain;
//         private CardPile _cardPile;
//         private TurnSystem _turnSystem;
//         private Timer _timer;
//         private WxkjTimer _wxkjTimer;
//         private CompeteTimer _competeTimer;
//         private CardPanel _cardPanel;
//         private List<Decision> decisions;

//         public bool isRunning;
//     }
// }