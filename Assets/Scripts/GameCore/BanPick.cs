using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameCore
{
    public class BanPick
    {
        public BanPick(Game game)
        {
            this.game = game;
        }
        private Game game;
        public List<General> Pool { get; private set; }
        public Dictionary<Team, List<General>> TeamPool { get; } = new Dictionary<Team, List<General>>
        {
            { Team.BLUE, new() },
            { Team.RED, new() }
        };

        public async Task Run()
        {
            var generalList = General.GetList();
            // Pool = AI.Shuffle(generalList, 18);
            Pool = generalList.Shuffle(18);
#if UNITY_EDITOR
            // string name = "夏侯惇";
            // General self = generalList.Find(x => x.name == name);
            // if (!Pool.Contains(self)) Pool[11] = self;
#endif

            game.eventSystem.SendToClient(new StartBanPick { generals = Pool.Select(x => x.id).ToList() });
            await Ban(Team.BLUE);
            await Ban(Team.RED);

            var current = Team.BLUE;
            while (Pool.Count > 0)
            {
                if (TeamPool[current].Count == TeamPool[~current].Count + 1) current = ~current;
                await Pick(current);
            }

            await SelfPick();

            game.eventSystem.SendToClient(new FinishBanPick { startFightTime = System.DateTime.Now });
        }

        private Player GetTeamPlayer(Team team) => game.AlivePlayers.Find(x => x.team == team);

        // private TaskCompletionSource<Model.BanpickMessage> tcs;
        private int second = 15;

        private async Task Ban(Team team)
        {
            game.eventSystem.SendToClient(new BanQuery
            {
                player = GetTeamPlayer(team).position,
                second = second
            });

            var cts = new CancellationTokenSource();
            BpAutoResult(team, cts.Token);

            var general = await WaitBp();
            cts.Cancel();
            cts.Dispose();
            Pool.Remove(general);


            game.eventSystem.SendToClient(new OnBan
            {
                player = GetTeamPlayer(team).position,
                general = general.id
            });
        }

        private async Task Pick(Team team)
        {
            game.eventSystem.SendToClient(new PickQuery
            {
                player = GetTeamPlayer(team).position,
                second = second
            });

            var cts = new CancellationTokenSource();
            BpAutoResult(team, cts.Token);

            var general = await WaitBp();
            cts.Cancel();
            cts.Dispose();

            Pool.Remove(general);
            TeamPool[team].Add(general);

            game.eventSystem.SendToClient(new OnPick
            {
                player = GetTeamPlayer(team).position,
                general = general.id
            });
        }

        private async Task<General> WaitBp()
        {
            var decision = await game.eventSystem.PopDecision() as GeneralDecision;
            return General.Get(decision.general);
        }

        private async void BpAutoResult(Team team, CancellationToken cancellationToken)
        {
            var player = GetTeamPlayer(team);

            try { await Delay.Run(player.isAI ? 100 : second * 1000, cancellationToken); }
            catch (TaskCanceledException) { return; }

            game.eventSystem.PushDecision(new GeneralDecision
            {
                player = player.position,
                general = Pool.First().id
            });
        }

        private async Task SelfPick()
        {
            second = 25;
            game.eventSystem.SendToClient(new StartSelfPick { second = second });

            var cts = new CancellationTokenSource();
            SelfAutoResult(cts.Token);
            AIAutoResult();

            await WaitSelfPick();
            cts.Cancel();
            cts.Dispose();
        }

        public async Task WaitSelfPick()
        {
            for (int i = 0; i < game.AlivePlayers.Count; i++)
            {
                var decision = await game.eventSystem.PopDecision() as GeneralDecision;
                var player = game.players[decision.player];
                generals.Add(player, General.Get(decision.general));
            }
        }

        public Dictionary<Player, General> generals { get; } = new();

        private async void SelfAutoResult(CancellationToken cancellationToken)
        {
            // if (!await new Delay(second).Run()) return;
            try { await Delay.Run(second * 1000, cancellationToken); }
            catch (TaskCanceledException) { return; }

            var player = game.AlivePlayers.Find(x => !x.isAI);
            int t = 0;
            foreach (var i in player.teammates)
            {
                game.eventSystem.PushDecision(new GeneralDecision
                {
                    player = i.position,
                    general = TeamPool[player.team][t++].id
                });
            }
        }

        private async void AIAutoResult()
        {
            var player = game.AlivePlayers.Find(x => x.isAI);
            if (player is null) return;
            // if (!await new Delay(0.1f).Run()) return;

            await Delay.Run(100);

            int t = 0;
            foreach (var i in player.teammates)
            {
                game.eventSystem.PushDecision(new GeneralDecision
                {
                    player = i.position,
                    general = TeamPool[player.team][t++].id
                });
            }
        }
    }
}