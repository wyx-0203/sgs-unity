using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Model;
using UnityEngine;

public class Connection : GlobalSingleton<Connection>
{
    private HubConnection hubConnection;
    private string currentUrl;

    public async Task JoinRoom(Mode mode)
    {
        // 从网关服务器获得房间服务器地址
        var json = await WebRequest.GetWithToken($"{Url.DOMAIN_NAME}joinRoom?mode={mode}");
        var message = json.DeSerialize<JoinRoomResponse>();
        var serverUrl = message.room_url;
        Debug.Log(serverUrl);

        // 连接房间服务器 (SignalR)
        if (serverUrl != currentUrl)
        {
            currentUrl = serverUrl;

            if (hubConnection != null) await hubConnection.DisposeAsync();
            hubConnection = new HubConnectionBuilder().WithUrl(serverUrl).Build();
            hubConnection.On("OnMessage", (string s) =>
            {
                Debug.Log(s);
                EventSystem.Instance.OnMessage(s.DeSerialize());
            });

            await hubConnection.StartAsync();
        }

        await hubConnection.SendAsync("JoinRoom", Global.Instance.userId);
    }

    public void ExitRoom() => hubConnection.SendAsync("ExitRoom");

    public void SetAlready(bool value) => hubConnection.SendAsync("SetAlready", value);

    public void StartGame() => hubConnection.SendAsync("StartGame");

    public void SendGameMessage(Message message) => hubConnection.SendAsync("SendGameMessage", message.Serialize());

    public void CheckRoom() => hubConnection?.SendAsync("CheckRoom");
}