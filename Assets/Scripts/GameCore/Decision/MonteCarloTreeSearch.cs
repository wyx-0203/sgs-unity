// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;

// namespace GameCore
// {
//     public class MCTS : Singleton<MCTS>
//     {
//         private class Node
//         {
//             public Node parent;
//             public List<Node> childs;
//             public bool isLeaf => childs is null;

//             public Team team;
//             // public List<Decision> state;
//             public Decision.DMessage decisionMessage;

//             // 
//             public float Q;
//             public int N;
//             public const float C = 2;
//             public float UCB => Q / N + C * Mathf.Sqrt(Mathf.Log(MCTS.Instance.root.N) / N);

//             public void AddChild(Node child)
//             {
//                 if (childs is null) childs = new List<Node>();
//                 childs.Add(child);
//                 child.parent = this;
//                 Debug.Log("mcts expand:" + child.decisionMessage);
//             }

//             public override string ToString()
//             {
//                 return "decision:\n" + decisionMessage + "\nucb=" + UCB;
//             }
//         }

//         public enum State
//         {
//             Disable,
//             Ready,
//             Restoring,
//             WaitTimer,
//             WaitCardPanel,
//             WaitShuffle,
//             Simulating
//         }

//         public async Task<Decision> Run(State _state)
//         {
//             root = new Node { N = 1, team = game.turnSystem.CurrentPlayer.team };
//             state = _state;
//             Expand(root);
//             if (root.childs.Count == 1) return root.childs[0].decisionMessage.ToDecision();

//             for (int i = 0; i < 1; i++)
//             {
//                 // Debug.Log("before select\n" + Util.GetGameInfo());
//                 Game.NewInstance();
//                 CardPile.NewInstance();
//                 TurnSystem.NewInstance();
//                 Timer.NewInstance();
//                 CardPanel.NewInstance();
//                 Decision.List.NewInstance();

//                 // Debug.Log(555);

//                 state = State.Restoring;
//                 await game.Init();
//                 // Debug.Log(666);

//                 // Decision.list = new List<Decision.Message>(_decisionList);
//                 Decision.List.Instance.AddRange(_decisionList);
//                 // Decision.list = decisions.Select(decision => new Decision
//                 // {
//                 //     action = decision.action,
//                 //     cards = decision.cards.Select(x => game.cardPile.cards[x.id]).ToList(),
//                 //     dests = decision.dests.Select(x => SgsMain.Instance.players[x.position]).ToList(),
//                 //     skill = decision.skill != null ? SgsMain.Instance.players[decision.skill.Src.position].FindSkill(decision.skill.Name) : null,
//                 //     converted = decision.converted,
//                 //     src = decision.src != null ? SgsMain.Instance.players[decision.src.position] : null,
//                 //     id = decision.id,
//                 // }).ToList();
//                 // Decision.index = 0;

//                 var node = Select();
//                 Debug.Log("select " + node);
//                 // Debug.Log("finish select\n" + Util.GetGameInfo());

//                 // Debug.Log("before restore");
//                 game.Run();
//                 while (state == State.Restoring) await Task.Yield();
//                 // Debug.Log("finish restore\n" + Util.GetGameInfo());

//                 // Decision next = null;
//                 if (node.N != 0)
//                 {
//                     // if (game.cardPile.PileCount == 0)
//                     // {
//                     //     var discards = AI.Shuffle(game.cardPile.DiscardPile, game.cardPile.DiscardPile.Count);
//                     //     next = new Decision { cards = discards };
//                     //     node.AddChild(new Node { decision = next });
//                     //     node = node.childs[0];
//                     //     // await Simulate(next);
//                     // }

//                     // else
//                     // {
//                     //     // Debug.Log("before expand");
//                     //     Expand(node);
//                     //     node = node.childs[0];
//                     //     next = node.decision;
//                     //     // Debug.Log("finish expand");
//                     //     // await Simulate(node.decision);
//                     // }
//                     Expand(node);
//                     node = node.childs[0];
//                 }
//                 // else next = Timer.Instance.DefaultAI();

//                 await Simulate(node.decisionMessage);
//                 // Debug.Log("finish simulation");

//                 node.Q = Evaluate(node);
//                 node.N = 1;
//                 BackPropagate(node);
//             }

//             Game.SetInstance(_sgsMain);
//             CardPile.SetInstance(_cardPile);
//             TurnSystem.SetInstance(_turnSystem);
//             Timer.SetInstance(_timer);
//             CardPanel.SetInstance(_cardPanel);
//             Decision.List.SetInstance(_decisionList);

//             var node1 = root.childs[0];
//             foreach (var j in root.childs) if (j.N > node1.N) node1 = j;
//             Debug.Log("mcts done");
//             return node1.decisionMessage.ToDecision();
//         }

//         private Node Select()
//         {
//             var node = root;
//             while (!node.isLeaf)
//             {
//                 var team = node.team;
//                 float maxUCB = float.MinValue;

//                 foreach (var i in node.childs)
//                 {
//                     if (i.N == 0) return i;

//                     float ucb = i.UCB * (i.team == team ? 1 : -1);
//                     if (ucb > maxUCB)
//                     {
//                         maxUCB = ucb;
//                         node = i;
//                     }
//                 }

//                 Decision.List.Instance.Push(node.decisionMessage);
//             }

//             // 

//             // if (node.N != 0)
//             // {
//             //     Expand(node);
//             //     node = node.childs[0];
//             // }
//             return node;
//         }

//         private void Expand(Node node)
//         {
//             switch (state)
//             {
//                 case State.WaitShuffle:

//                     var discards = AI.Shuffle(game.cardPile.DiscardPile, game.cardPile.DiscardPile.Count);
//                     var decision = new Decision { cards = discards };
//                     node.AddChild(new Node { decisionMessage = decision.ToMessage() });
//                     break;

//                 case State.WaitCardPanel:

//                     // if(CardPanel)
//                     validCards = new List<Card>(CardPanel.Instance.cards);
//                     player = CardPanel.Instance.player;
//                     var dest = CardPanel.Instance.dest;

//                     if (player.team != dest.team)
//                     {
//                         var handcard = validCards.FirstOrDefault(x => x.isHandCard);
//                         if (handcard != null)
//                         {
//                             validCards = validCards.Where(x => !x.isHandCard).ToList();
//                             validCards.Add(handcard);
//                         }
//                     }

//                     foreach (var i in validCards)
//                     {
//                         var decision1 = new Decision { action = true, cards = new List<Card> { i } };
//                         node.AddChild(new Node { team = player.team, decisionMessage = decision1.ToMessage() });
//                     }
//                     break;

//                 case State.WaitTimer:

//                     selectedNode = node;
//                     player = Timer.Instance.players[0];
//                     temp = Timer.Instance.temp;
//                     temp.action = true;

//                     // var cards = player.cards;
//                     validCards = player.cards.Where(x => Timer.Instance.isValidCard(x)).ToList();

//                     // if (Timer.Instance.multiConvert.Count > 0)
//                     // {
//                     foreach (var i in Timer.Instance.multiConvert.Where(x => Timer.Instance.isValidCard(x)))
//                     {
//                         temp.cards.Add(i);
//                         // temp.
//                         dests = game.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).ToList();
//                         int minDest = Timer.Instance.minDest();
//                         int maxDest = Mathf.Min(Timer.Instance.maxDest(), dests.Count);

//                         // if (minDest == 2 && maxDest == 2) Traverse2Dests();
//                         for (int j = minDest; j <= maxDest; j++) TraverseDests(j);
//                         temp.cards.Remove(i);
//                     }
//                     // }

//                     if (Timer.Instance.type == Timer.Type.WXKJ)
//                     {
//                         // validCards = new List<Card>();
//                         foreach (var i in player.team.GetAllPlayers().Where(x => x != player))
//                         {
//                             validCards.AddRange(i.handCards.Where(x => Timer.Instance.isValidCard(x)));
//                         }
//                     }
//                     // else validCards = player.HandCards.Union(player.Equipments.Values).Where(x => Timer.Instance.isValidCard(x)).ToList();

//                     int minCard = Timer.Instance.minCard;
//                     int maxCard = Mathf.Min(Timer.Instance.maxCard, validCards.Count);
//                     for (int i = minCard; i <= maxCard; i++) TraverseCards(i);

//                     foreach (var i in player.skills.Where(x => x.IsValid && x is not Triggered))
//                     {
//                         temp.skill = i;
//                         validCards = player.cards.Where(x => Timer.Instance.isValidCard(x)).ToList();
//                         minCard = Timer.Instance.minCard;
//                         maxCard = Mathf.Min(Timer.Instance.maxCard, validCards.Count);
//                         for (int j = minCard; j <= maxCard; j++) TraverseCards(j);
//                     }
//                     // temp.action = false;

//                     if (Timer.Instance.refusable) node.AddChild(new Node
//                     {
//                         team = player.team,
//                         decisionMessage = new Decision().ToMessage()
//                     });

//                     break;
//             }

//         }

//         // private Node nodeForExpand;

//         private async Task Simulate(Decision.DMessage decision)
//         {
//             state = State.Simulating;
//             Decision.List.Instance.Push(decision);
//             while (state != State.Ready) await Task.Yield();
//         }

//         private float Evaluate(Node node)
//         {
//             float sum = 0;
//             foreach (var i in game.AlivePlayers)
//             {
//                 float value = i.handCardsCount;
//                 value += i.Equipments.Values.Count * 1.5f;
//                 value -= i.JudgeCards.Count * 2;
//                 value += i.hp * 1.6f;
//                 sum += i.team == node.team ? value : -value;
//             }
//             Debug.Log("Q=" + sum + "\nnode=" + node);
//             return sum;
//         }

//         private void BackPropagate(Node node)
//         {
//             Node parent = node.parent;
//             while (parent != null)
//             {
//                 parent.Q += node.team == parent.team ? node.Q : -node.Q;
//                 parent.N++;
//                 parent = parent.parent;
//             }
//         }

//         private void TraverseCards(int length, int index = 0)
//         {
//             if (length == 0)
//             {
//                 if (Timer.Instance.type == Timer.Type.WXKJ) temp.src = temp.cards.FirstOrDefault()?.src;
//                 // temp.converted = (temp.skill as Converted)?.Convert(temp.cards);

//                 dests = game.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).ToList();
//                 int minDest = Timer.Instance.minDest();
//                 int maxDest = Mathf.Min(Timer.Instance.maxDest(), dests.Count);

//                 if (minDest == 2 && maxDest == 2) Traverse2Dests();
//                 else for (int i = minDest; i <= maxDest; i++) TraverseDests(i);

//                 // if (temp.skill is Converted) temp.cards = temp.cards[0].PrimiTives;
//                 return;
//             }

//             for (int i = index; i < validCards.Count; i++)
//             {
//                 if (!Timer.Instance.isValidCard(validCards[i])) continue;

//                 temp.cards.Add(validCards[i]);
//                 TraverseCards(length - 1, i + 1);
//                 temp.cards.Remove(validCards[i]);
//             }
//         }

//         private List<Card> validCards;
//         private List<Player> dests;
//         private Decision temp;
//         private Node selectedNode;
//         private Player player;
//         // private List<Card> subsequence = new();

//         private void TraverseDests(int length, int index = 0)
//         {
//             if (length == 0)
//             {
//                 selectedNode.AddChild(new Node
//                 {
//                     team = player.team,
//                     decisionMessage = temp.ToMessage()
//                 });
//                 return;
//             }
//             // while(temp.dests.Count)
//             // var dests = 
//             for (int i = index; i < dests.Count; i++)
//             {
//                 temp.dests.Add(dests[i]);
//                 TraverseDests(length - 1, i + 1);
//                 temp.dests.Remove(dests[i]);
//             }
//         }

//         private void Traverse2Dests()
//         {
//             // var firstDests = SgsMain.Instance.AlivePlayers.Where(x => Timer.Instance.isValidDest(x)).ToList();
//             foreach (var i in dests)
//             {
//                 temp.dests.Add(i);
//                 var secondDests = game.AlivePlayers.Where(x => Timer.Instance.isValidDest(x) && x != i);

//                 foreach (var j in secondDests)
//                 {
//                     temp.dests.Add(j);

//                     selectedNode.AddChild(new Node
//                     {
//                         team = player.team,
//                         decisionMessage = temp.ToMessage()
//                     });
//                     // 
//                     temp.dests.Remove(j);
//                 }
//                 temp.dests.Remove(i);
//             }
//         }

//         public MCTS()
//         {
//             state = Config.Instance.EnableMCTS && Room.Instance.IsSingle ? State.Ready : State.Disable;

//             _sgsMain = game;
//             _cardPile = game.cardPile;
//             _turnSystem = game.turnSystem;
//             _timer = Timer.Instance;
//             _cardPanel = CardPanel.Instance;
//             _decisionList = Decision.List.Instance;
//         }

//         private Game _sgsMain;
//         private CardPile _cardPile;
//         private TurnSystem _turnSystem;
//         public Timer _timer;
//         private CardPanel _cardPanel;
//         public Decision.List _decisionList;

//         // public bool enable;
//         // public bool simulating;
//         public State state;
//         public bool isRunning => state == State.Restoring || state == State.Simulating;

//         private Node root;
//     }
// }