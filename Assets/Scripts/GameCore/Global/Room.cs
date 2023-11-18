using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCore
{
    public class Room : GlobalSingleton<Room>
    {
        // 单机模式
        public bool IsSingle { get; set; } = true;
        public Mode mode { get; private set; } = new ThreeVSThree();
        public UserJson[] Users { get; private set; }
        public UserJson self { get; private set; }
        public List<int> players { get; private set; }

        public async void JoinRoom(string mode)
        {
            var msg = await WebRequest.GetWithToken(Url.DOMAIN_NAME + "joinRoom?mode=" + mode);
            var json = JsonUtility.FromJson<JoinRoomResponse>(msg);

            this.mode = Mode.modeMap[json.mode];
            Users = new UserJson[2];
            foreach (var i in json.players)
            {
                Users[i.position] = i;
                // Debug.Log(i.id);
                // Debug.Log(Self.Instance.UserId);
            }
            self = Users.ToList().Find(x => x != null && x.id == Self.Instance.UserId);
            Users[json.owner_pos].owner = true;

            JoinRoomView();
        }

        public async void ExitRoom()
        {
            var msg = await WebRequest.GetWithToken(Url.DOMAIN_NAME + "exitRoom");
            var json = JsonUtility.FromJson<HttpResponse>(msg);
            if (json.code != 0) return;

            Users = null;
            ExitRoomView();
        }

        public async void AddPlayer(UserJson user)
        {
            while (self is null) await Task.Yield();

            if (user.id == self.id) return;

            Users[user.position] = user;

            AddPlayerView(user);
        }

        public void RemovePlayer(removePlayerMessage json)
        {
            if (json.position == self.position) return;

            Users[json.position] = null;

            Users[json.owner_pos].owner = true;
            RemovePlayerView(json.position, json.owner_pos);
        }

        public async void SendSetAlready()
        {
            await WebRequest.GetWithToken(Url.DOMAIN_NAME + "setAlready");
        }

        public void SetAlready(SetAlreadyMessage json)
        {
            Users[json.position].already = json.already;
            SetAlreadyView(Users[json.position], json.already);
        }

        public async void SendStartGame()
        {
            await WebRequest.GetWithToken(Url.DOMAIN_NAME + "startGame");
        }

        public void StartGame(StartGameMessage json)
        {
            // 从服务器获得userid列表，以确定位置
            Self.Instance.team = json.first_id == Self.Instance.UserId ? Team.BLUE : Team.RED;

            IsSingle = false;
            StartGameView?.Invoke();
        }


        public Action JoinRoomView { get; set; }
        public Action ExitRoomView { get; set; }
        public Action<UserJson> AddPlayerView { get; set; }
        public Action<int, int> RemovePlayerView { get; set; }
        public Action<UserJson, bool> SetAlreadyView { get; set; }
        public Action StartGameView { get; set; }
    }
}