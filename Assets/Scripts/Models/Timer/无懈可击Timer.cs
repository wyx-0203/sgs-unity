// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;
// using System.Threading.Tasks;
// using System;
// using System.Linq;

// namespace Model
// {
//     /// <summary>
//     /// 用于暂停主线程，并获得玩家的操作结果
//     /// </summary>
//     public class 无懈可击Timer : Timer
//     {
//         public 无懈可击Timer()
//         {
//             maxCard = 1;
//             minCard = 1;
//             IsValidCard = card => card is 无懈可击;
//         }

//         // #region singleton
//         // private static 无懈可击Timer instance;
//         // public new static 无懈可击Timer Instance
//         // {


//         //     get
//         //     {
//         //         if (instance is null)
//         //         {
//         //             instance = new 无懈可击Timer();
//         //             UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
//         //         }
//         //         return instance;
//         //     }
//         // }

//         // private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
//         // {
//         //     UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
//         //     instance = default;
//         // }
//         // #endregion

//         // public Player player { get; private set; }

//         /// <summary>
//         /// 传入已选中的卡牌与目标，通过设置TaskCompletionSource返回值，继续主线程
//         /// </summary>
//         public static void SetResult(List<int> cards)
//         {
//             foreach (var id in cards)Instance. Cards.Add(CardPile.Instance.cards[id]);
//         }

//         public static async Task<bool> WaitResult()
//         {
//             TimerMessage json;
//             if (Room.Instance.IsSingle)
//             {
//                 // 若为单机模式，则通过tcs阻塞线程，等待操作结果
//                Instance. waitAction = new TaskCompletionSource<TimerMessage>();
//                 json = await waitAction.Task;
//             }
//             else
//             {
//                 // 若为多人模式，则等待ws通道传入消息
//                 var message = await WS.Instance.PopMsg();
//                 json = JsonUtility.FromJson<TimerMessage>(message);
//             }

//             if (json.result)
//             {
//                 player = SgsMain.Instance.Players[json.src];
//                 SetResult(json.cards);
//             }
//             else
//             {
//                 isDone++;
//                 if (isDone < SgsMain.Instance.AlivePlayers.Count) return await WaitResult();
//             }

//             Delay.StopAll();
//             return json.result;
//         }


//         private int isDone;
//         public async Task<bool> Run()
//         {
//             // maxCard = 1;
//             // minCard = 1;
//             // isWxkj = true;
//             // IsValidCard = card => card is 无懈可击;

//             Cards = new List<Card>();
//             // Dests = new List<Player>();
//             // Cards = new List<Card>();

//             isDone = 0;

//             StartTimerView?.Invoke(this);
//             SelfAutoResult();
//             if (Room.Instance.IsSingle) AIAutoResult();
//             bool result = await WaitResult();

//             StopTimerView?.Invoke(this);

//             // IsValidCard = card => !card.IsConvert;

//             return result;
//         }

//         public void SendResult(int src, bool result, List<int> cards = null)
//         {
//             var json = new TimerMessage
//             {
//                 msg_type = "wxkj_set_result",
//                 result = result,
//                 cards = cards,
//                 src = src,
//             };

//             if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
//             else
//             {
//                 Delay.StopAll();
//                 WS.Instance.SendJson(json);
//             }
//         }

//         private async void AIAutoResult()
//         {
//             if (!await new Delay(1).Run()) return;

//             foreach (var i in SgsMain.Instance.AlivePlayers)
//             {
//                 if (i.isAI) SendResult(i.Position, false, null);
//             }
//         }

//         private async void SelfAutoResult()
//         {
//             if (!await new Delay(second).Run()) return;

//             foreach (var i in SgsMain.Instance.AlivePlayers)
//             {
//                 if (i.isSelf) SendResult(i.Position, false);
//             }
//         }

//         // private UnityAction<Player> moveSeat;
//     }
// }