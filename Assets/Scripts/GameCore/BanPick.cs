using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GameCore
{
    public class BanPick : Singleton<BanPick>
    {
        public List<General> Pool { get; private set; }
        public Dictionary<Team, List<General>> TeamPool { get; } = new Dictionary<Team, List<General>>
        {
            { Team.BLUE, new() },
            { Team.RED, new() }
        };

        public async Task Run()
        {
            var generalList = await General.GetList();
            Pool = AI.Shuffle(generalList, 18);
#if UNITY_EDITOR
            // string name = "夏侯惇";
            // General self = generalList.Find(x => x.name == name);
            // if (!Pool.Contains(self)) Pool[11] = self;
#endif

            EventSystem.Instance.Send(new StartBanPick { generals = Pool.Select(x => x.id).ToList() });
            await Ban(Team.BLUE);
            await Ban(Team.RED);

            var current = Team.BLUE;
            while (Pool.Count > 0)
            {
                if (TeamPool[current].Count == TeamPool[~current].Count + 1) current = ~current;
                await Pick(current);
            }

            await SelfPick();

            EventSystem.Instance.Send(new FinishBanPick());
        }

        private Player GetTeamPlayer(Team team) => Game.Instance.AlivePlayers.Find(x => x.team == team);

        private TaskCompletionSource<Model.BanpickMessage> tcs;
        private int second = 15;

        private async Task Ban(Team team)
        {
            BpAutoResult(team);
            EventSystem.Instance.Send(new BanQuery
            {
                player = GetTeamPlayer(team).position,
                second = second
            });
            var general = await WaitBp();
            Pool.Remove(general);

            EventSystem.Instance.Send(new OnBan
            {
                player = GetTeamPlayer(team).position,
                general = general.id
            });
            Delay.StopAll();
        }

        private async Task Pick(Team team)
        {
            BpAutoResult(team);
            EventSystem.Instance.Send(new PickQuery
            {
                player = GetTeamPlayer(team).position,
                second = second
            });
            var general = await WaitBp();
            Pool.Remove(general);
            TeamPool[team].Add(general);

            EventSystem.Instance.Send(new OnPick
            {
                player = GetTeamPlayer(team).position,
                general = general.id
            });
            Delay.StopAll();
        }

        private async Task<General> WaitBp()
        {
            var decision = await EventSystem.Instance.PopDecision() as GeneralDecision;
            return General.Get(decision.general);
        }

        private async void BpAutoResult(Team team)
        {
            var player = GetTeamPlayer(team);
            if (!await new Delay(player.isAI ? 0.1f : second).Run()) return;
            EventSystem.Instance.PushDecision(new GeneralDecision
            {
                player = player.position,
                general = Pool.First().id
            });
        }

        private async Task SelfPick()
        {
            second = 25;
            EventSystem.Instance.Send(new StartSelfPick { second = second });
            SelfAutoResult();
            AIAutoResult();
            await WaitSelfPick();
            Delay.StopAll();
        }
        public async Task WaitSelfPick()
        {
            for (int i = 0; i < Game.Instance.AlivePlayers.Count; i++)
            {
                var decision = await EventSystem.Instance.PopDecision() as GeneralDecision;
                var player = Game.Instance.players[decision.player];
                generals.Add(player, General.Get(decision.general));
            }
        }

        public Dictionary<Player, General> generals { get; } = new();

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            var player = Game.Instance.AlivePlayers.Find(x => !x.isAI);
            int t = 0;
            foreach (var i in player.teammates)
            {
                EventSystem.Instance.PushDecision(new GeneralDecision
                {
                    player = i.position,
                    general = TeamPool[player.team][t++].id
                });
            }
        }

        private async void AIAutoResult()
        {
            var player = Game.Instance.AlivePlayers.Find(x => x.isAI);
            if (player is null) return;
            if (!await new Delay(0.1f).Run()) return;
            int t = 0;
            foreach (var i in player.teammates)
            {
                EventSystem.Instance.PushDecision(new GeneralDecision
                {
                    player = i.position,
                    general = TeamPool[player.team][t++].id
                });
            }
        }
    }
}