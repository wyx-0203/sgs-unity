using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
// using NativeWebSocket;

public class WebSocket : GlobalSingletonMono<WebSocket>
{
    private NativeWebSocket.WebSocket websocket;

    public async void Connect(string userId)
    {
        if (websocket != null && websocket.State is NativeWebSocket.WebSocketState.Open) return;

        websocket = new NativeWebSocket.WebSocket(Url.WEB_SOCKET + "?user_id=" + userId);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        // websocket.OnMessage += (bytes) =>
        // {
        //     var message = System.Text.Encoding.UTF8.GetString(bytes);
        //     var msgType = JsonUtility.FromJson<Model.Message>(message).msgType;
        //     Debug.Log("OnMessage! " + message);

        //     switch (msgType)
        //     {
        //         case "add_player":
        //             var playerJson = JsonUtility.FromJson<Model.AddPlayerMessage>(message).user;
        //             GameCore.Room.Instance.AddPlayer(playerJson);
        //             break;

        //         case "remove_player":
        //             var removePlayerMessage = JsonUtility.FromJson<Model.removePlayerMessage>(message);
        //             GameCore.Room.Instance.RemovePlayer(removePlayerMessage);
        //             break;

        //         case "set_already":
        //             var setAlreadyMessage = JsonUtility.FromJson<Model.SetAlreadyMessage>(message);
        //             GameCore.Room.Instance.SetAlready(setAlreadyMessage);
        //             break;

        //         case "start_game":
        //             messages.Clear();
        //             var startGameMessage = JsonUtility.FromJson<Model.InitPlayer>(message);
        //             GameCore.Room.Instance.StartGame(startGameMessage);
        //             break;

        //         // case "surrender":
        //         //     var team = JsonUtility.FromJson<SurrenderMessage>(message).team;
        //         //     Model.GameOver.Instance.Surrender(team);
        //         //     break;

        //         case "change_skin":
        //             var changeSkinMessage = JsonUtility.FromJson<Model.ChangeSkinMessage>(message);
        //             GameCore.game.players[changeSkinMessage.position].ChangeSkin(changeSkinMessage.skin_id);
        //             break;

        //         default:
        //             messages.Add(message);
        //             break;
        //     }
        // };

        await websocket.Connect();
    }

    private List<string> messages = new List<string>();

    public async Task<string> PopMessage()
    {
        while (messages.Count == 0) await Task.Yield();
        var msg = messages[0];
        messages.RemoveAt(0);
        return msg;
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public async void Send(string message)
    {
        // var message = JsonUtility.ToJson(json);
        Debug.Log("send:" + message);
        if (websocket.State == NativeWebSocket.WebSocketState.Open) await websocket.SendText(message);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}