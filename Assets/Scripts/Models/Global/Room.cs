using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class Room : GlobalSingleton<Room>
    {
        // 单机模式
        public bool IsSingle { get; set; } = true;
        public Mode mode { get; private set; } = Mode.统帅双军;
        public User[] Users { get; private set; }
        public User self { get; private set; }
        public List<int> players { get; private set; }

        public async void JoinRoom(Mode mode)
        {
            var msg = await WebRequest.GetWithToken(Url.DOMAIN_NAME + "joinRoom?mode=" + ((int)mode).ToString());
            var json = JsonUtility.FromJson<JoinRoomResponse>(msg);

            this.mode = json.mode;
            Users = new User[mode is Mode.欢乐成双 ? 4 : 2];
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

        public async void AddPlayer(User user)
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
            players = json.players;

            // 若为统帅模式，则补充三、四号位
            if (mode is Mode.统帅双军)
            {
                players.Add(players[1]);
                players.Add(players[0]);
            }
            IsSingle = false;
            StartGameView();
        }


        public UnityAction JoinRoomView { get; set; }
        public UnityAction ExitRoomView { get; set; }
        public UnityAction<User> AddPlayerView { get; set; }
        public UnityAction<int, int> RemovePlayerView { get; set; }
        public UnityAction<User, bool> SetAlreadyView { get; set; }
        public UnityAction StartGameView { get; set; }
    }
}