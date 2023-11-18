using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCore
{
    public class BanPick : Singleton<BanPick>
    {
        public List<General> Pool { get; private set; }
        public Dictionary<Team, List<General>> TeamPool { get; } = new();

        public async Task Run()
        {
            // string url = Url.JSON + "general.json";
            var generalList = await General.GetList();

            if (Room.Instance.IsSingle)
            {
                Pool = AI.Shuffle(generalList, 18);
#if UNITY_EDITOR
                // string name = "夏侯惇";
                // General self = generalList.Find(x => x.name == name);
                // if (!Pool.Contains(self)) Pool[11] = self;
#endif
            }
            else
            {
                List<int> generalIds;

                if (Main.Instance.players[0].isSelf)
                {
                    generalIds = generalList.OrderBy(x => UnityEngine.Random.value).Take(12).Select(x => x.id).ToList();
                    var json = new GeneralPoolMessage
                    {
                        msg_type = "general_pool",
                        generals = generalIds
                    };
                    WebSocket.Instance.SendMessage(json);
                }

                generalIds = JsonUtility.FromJson<GeneralPoolMessage>(await WebSocket.Instance.PopMessage()).generals;
                Pool = generalIds.Select(x => generalList.Find(y => y.id == x)).ToList();
            }

            TeamPool.Add(Team.BLUE, new());
            TeamPool.Add(Team.RED, new());

            ShowPanelView?.Invoke();

            await Task.Yield();

            Current = Team.BLUE;
            await Ban();

            Current = Team.RED;
            await Ban();
            Current = Team.BLUE;
            while (Pool.Count > 0)
            {
                if (TeamPool[Current].Count == TeamPool[!Current].Count + 1) Current = !Current;
                await Pick();
            }

            await SelfPick();
        }


        private TaskCompletionSource<BanpickMessage> tcs;
        public Team Current { get; private set; }
        public int second => 15;

        private async Task Ban()
        {
            if (Current == Self.Instance.team || Room.Instance.IsSingle) BpAutoResult();
            StartBanView();
            int id = await WaitBp();
            var general = Pool.Find(x => x.id == id);
            Pool.Remove(general);

            while (OnBanView is null) await Task.Yield();
            OnBanView?.Invoke(general);
            Delay.StopAll();
        }

        private async Task Pick()
        {
            if (Current == Self.Instance.team || Room.Instance.IsSingle) BpAutoResult();
            StartPickView();
            int id = await WaitBp();
            var general = Pool.Find(x => x.id == id);
            Pool.Remove(general);

            TeamPool[Current].Add(general);
            OnPickView?.Invoke(general);
            Delay.StopAll();
        }

        public void SendBpResult(int general)
        {
            var json = new BanpickMessage
            {
                msg_type = "ban_pick_result",
                generals = new List<int> { general },
            };

            if (Room.Instance.IsSingle) tcs.TrySetResult(json);
            else WebSocket.Instance.SendMessage(json);
        }

        private async Task<int> WaitBp()
        {
            BanpickMessage json;
            if (Room.Instance.IsSingle)
            {
                tcs = new TaskCompletionSource<BanpickMessage>();
                json = await tcs.Task;
            }
            else
            {
                var msg = await WebSocket.Instance.PopMessage();
                json = JsonUtility.FromJson<BanpickMessage>(msg);
            }

            return json.generals[0];
        }

        private async void BpAutoResult()
        {
            if (!await new Delay(Current == Self.Instance.team ? second : 0.1f).Run()) return;
            SendBpResult(Pool[0].id);
        }

        private async Task SelfPick()
        {
            StartSelfPickView?.Invoke();
            SelfAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            await WaitSelfPick();
            await WaitSelfPick();
            Delay.StopAll();
        }

        public void SendSelfResult(Team team, List<int> generals)
        {
            var json = new BanpickMessage
            {
                msg_type = "self_pick_result",
                team = team.value,
                generals = generals,
            };

            if (Room.Instance.IsSingle) tcs.TrySetResult(json);
            else WebSocket.Instance.SendMessage(json);
        }

        public async Task WaitSelfPick()
        {
            BanpickMessage json;
            if (Room.Instance.IsSingle)
            {
                tcs = new TaskCompletionSource<BanpickMessage>();
                json = await tcs.Task;
            }
            else
            {
                var msg = await WebSocket.Instance.PopMessage();
                json = JsonUtility.FromJson<BanpickMessage>(msg);
            }

            Team team = json.team ? Team.RED : Team.BLUE;
            var players = team.GetAllPlayers().ToArray();
            for (int i = 0; i < players.Length; i++) generals.Add(players[i], TeamPool[team].Find(x => x.id == json.generals[i]));
        }

        public Dictionary<Player, General> generals { get; } = new();

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            var team = Self.Instance.team;
            SendSelfResult(team, TeamPool[team].Select(x => x.id).Take(team.GetAllPlayers().Count()).ToList());
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(0.1f).Run()) return;

            var team = !Self.Instance.team;
            SendSelfResult(team, TeamPool[team].Select(x => x.id).OrderBy(x => UnityEngine.Random.value).Take(team.GetAllPlayers().Count()).ToList());
        }

        public Action ShowPanelView;
        public Action StartBanView;
        public Action<General> OnBanView;
        public Action StartPickView;
        public Action<General> OnPickView;
        public Action StartSelfPickView;
    }
}