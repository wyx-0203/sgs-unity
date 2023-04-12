using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class BanPick : Singleton<BanPick>
    {
        public List<General> Pool { get; private set; }
        public Dictionary<bool, Dictionary<int, General>> TeamPool { get; private set; }

        private Mode mode;
        private Player[] players => SgsMain.Instance.players;

        public async Task Run()
        {
            string url = Url.JSON + "general.json";
            List<General> generalList = JsonList<General>.FromJson(await WebRequest.Get(url));

            if (Room.Instance.IsSingle)
            {
                Pool = generalList.OrderBy(x => Random.value).Take(12).ToList();
#if UNITY_EDITOR
                string name = "夏侯惇";
                General self = generalList.Find(x => x.name == name);
                if (!Pool.Contains(self)) Pool[11] = self;
#endif
            }
            else
            {
                List<int> generalIds;

                if (SgsMain.Instance.players[0].isSelf)
                {
                    generalIds = generalList.OrderBy(x => Random.value).Take(12).Select(x => x.id).ToList();
                    var json = new GeneralPoolMessage
                    {
                        msg_type = "general_pool",
                        generals = generalIds
                    };
                    WS.Instance.SendJson(json);
                }

                generalIds = JsonUtility.FromJson<GeneralPoolMessage>(await WS.Instance.PopMsg()).generals;
                Pool = generalIds.Select(x => generalList.Find(y => y.id == x)).ToList();
            }

            TeamPool = new Dictionary<bool, Dictionary<int, General>>
            {
                { Team.BLUE, new Dictionary<int, General>()},
                { Team.RED, new Dictionary<int, General>()}
            };

            ShowPanelView?.Invoke();

            await Task.Yield();

            Current = SgsMain.Instance.players[3];
            await Ban();

            Current = SgsMain.Instance.players[2];
            await Ban();
            Current = SgsMain.Instance.players[0];
            while (Pool.Count > 0)
            {
                await Pick();
            }

            await SelfPick();
        }


        private TaskCompletionSource<BanpickMessage> tcs;
        public Player Current { get; private set; }
        public int second => 15;

        private async Task Ban()
        {
            if (Current.isSelf || Room.Instance.IsSingle) BpAutoResult();
            StartBanView();
            int id = await WaitBp();
            var general = Pool.Find(x => x.id == id);
            Pool.Remove(general);

            while (OnBanView is null) await Task.Yield();
            OnBanView(id);
            Delay.StopAll();
        }

        private async Task Pick()
        {
            if (Current.isSelf || Room.Instance.IsSingle) BpAutoResult();
            StartPickView();
            int id = await WaitBp();
            var general = Pool.Find(x => x.id == id);
            Pool.Remove(general);

            TeamPool[Current.team].Add(id, general);
            OnPickView(id);
            Delay.StopAll();
            Current = Current.next;
        }

        public void SendBpResult(int general)
        {
            var json = new BanpickMessage
            {
                msg_type = "ban_pick_result",
                general = general,
            };

            if (Room.Instance.IsSingle) tcs.TrySetResult(json);
            else WS.Instance.SendJson(json);
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
                var msg = await WS.Instance.PopMsg();
                json = JsonUtility.FromJson<BanpickMessage>(msg);
            }

            return json.general;
        }

        private async void BpAutoResult()
        {
            if (!await new Delay(Current.isSelf ? second : 1).Run()) return;
            SendBpResult(Pool[0].id);
        }

        private async Task SelfPick()
        {
            StartSelfPickView();
            SelfAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            for (int i = 0; i < 4; i++) await WaitSelfPick();
            Delay.StopAll();
        }

        public void SendSelfResult(int position, int general)
        {
            var json = new BanpickMessage
            {
                msg_type = "self_pick_result",
                position = position,
                general = general,
            };

            if (Room.Instance.IsSingle) tcs.TrySetResult(json);
            else WS.Instance.SendJson(json);
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
                var msg = await WS.Instance.PopMsg();
                json = JsonUtility.FromJson<BanpickMessage>(msg);
            }

            var general = TeamPool[SgsMain.Instance.players[json.position].team][json.general];
            SgsMain.Instance.players[json.position].InitGeneral(general);

            SelfPickView?.Invoke(json.position);
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            var team = Self.Instance.team;
            var list = TeamPool[team].Keys.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < 4; i++)
            {
                if (!SgsMain.Instance.players[i].isSelf) continue;
                SendSelfResult(i, list[i]);
            }
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;

            var team = !Self.Instance.team;
            var list = TeamPool[team].Keys.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < 4; i++)
            {
                if (SgsMain.Instance.players[i].team != team) continue;
                SendSelfResult(i, list[i]);
            }
        }

        public UnityAction ShowPanelView;
        public UnityAction StartBanView;
        public UnityAction<int> OnBanView;
        public UnityAction StartPickView;
        public UnityAction<int> OnPickView;
        public UnityAction<int> SelfPickView;
        public UnityAction StartSelfPickView;
    }
}