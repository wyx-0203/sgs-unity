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
//     public class TimerBase<T> : Singleton<T> where T:new()
//     {
//         // 单机模式中使用tcs阻塞线程
//         protected TaskCompletionSource<TimerMessage> waitAction;

//         public int maxCard { get; set; } = 0;
//         public int minCard { get; set; } = 0;
//         public Func<int> MaxDest { get; set; }
//         public Func<int> MinDest { get; set; }
//         public Func<Card, bool> IsValidCard { get; set; } = card => !card.IsConvert;
//         public Func<Player, bool> IsValidDest { get; set; } = dest => true;

//         public string Hint { get; set; }
//         // public string GivenSkill { get; set; } = "";

//         public int second;
//         public List<Card> Cards { get; set; }

//         public UnityAction<T> StartTimerView;
//         public UnityAction<T> StopTimerView;
//     }
// }