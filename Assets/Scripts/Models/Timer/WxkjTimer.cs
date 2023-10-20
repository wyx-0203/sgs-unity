// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine.Events;

// namespace Model
// {
//     /// <summary>
//     /// 用于暂停主线程，并获得玩家的操作结果
//     /// </summary>
//     public class WxkjTimer : Timer
//     {
//         public WxkjTimer()
//         {
//             second = 10;

//             maxCard = 1;
//             minCard = 1;
//             maxDest = () => 0;
//             minDest = () => 0;
//             isValidCard = card => card is 无懈可击;
//             DefaultAI = () =>
//             {
//                 foreach (var i in players)
//                 {
//                     var card = i.FindCard<无懈可击>();
//                     if (card is null || scheme.Src.team == team) continue;

//                     return new Decision { src = i, action = true, cards = new List<Card> { card } };
//                 }
//                 return new();
//             };
//         }

//         private static WxkjTimer instance;
//         public static new WxkjTimer Instance
//         {
//             get
//             {
//                 if (instance is null) instance = new WxkjTimer();
//                 return instance;
//             }
//         }

//         private Card scheme;
//         private Team team;

//         public async Task<Decision> Run(Card scheme, Team team)
//         {
//             currentInstance = instance;
//             this.scheme = scheme;
//             this.team = team;
//             players = SgsMain.Instance.AlivePlayers.Where(x => x.team == team).ToList();
//             hint = scheme + "即将对" + scheme.CurrentDest + "生效，是否使用无懈可击？";

//             StartTimerView?.Invoke();
//             await AutoDecision();
//             var decision = await WaitResult();

//             temp = new Decision();
//             StopTimerView?.Invoke();
//             currentInstance = null;
//             return decision;
//         }

//         private new UnityAction StartTimerView => Singleton<Timer>.Instance.StartTimerView;
//         private new UnityAction StopTimerView => Singleton<Timer>.Instance.StopTimerView;

//         public static new void RemoveInstance()
//         {
//             if (currentInstance == instance)
//             {
//                 instance.isRunning = true;
//                 currentInstance = null;
//             }
//             instance = new();
//         }
//         public static void RestoreInstance(WxkjTimer _instance)
//         {
//             instance = _instance;
//             if (instance.isRunning)
//             {
//                 instance.isRunning = false;
//                 currentInstance = instance;
//             }
//         }
//         private bool isRunning;
//     }
// }