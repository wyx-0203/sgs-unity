// using System.Collections.Generic;
// using UnityEngine;
// using System.Threading.Tasks;

// public class WebSocket : GlobalSingletonMono<WebSocket>
// {
//     private NativeWebSocket.WebSocket websocket;

//     public async void Connect(string userId)
//     {
//         if (websocket != null && websocket.State is NativeWebSocket.WebSocketState.Open) return;

//         websocket = new NativeWebSocket.WebSocket(Url.WEB_SOCKET + "?user_id=" + userId);

//         websocket.OnOpen += () =>
//         {
//             Debug.Log("Connection open!");
//         };

//         websocket.OnError += (e) =>
//         {
//             Debug.Log("Error! " + e);
//         };

//         websocket.OnClose += (e) =>
//         {
//             Debug.Log("Connection closed!");
//         };

//         await websocket.Connect();
//     }

//     private List<string> messages = new List<string>();

//     public async Task<string> PopMessage()
//     {
//         while (messages.Count == 0) await Task.Yield();
//         var msg = messages[0];
//         messages.RemoveAt(0);
//         return msg;
//     }

//     private void Update()
//     {
// #if !UNITY_WEBGL || UNITY_EDITOR
//         websocket.DispatchMessageQueue();
// #endif
//     }

//     public async void Send(string message)
//     {
//         // var message = JsonUtility.ToJson(json);
//         Debug.Log("send:" + message);
//         if (websocket.State == NativeWebSocket.WebSocketState.Open) await websocket.SendText(message);
//     }

//     private async void OnApplicationQuit()
//     {
//         await websocket.Close();
//     }
// }