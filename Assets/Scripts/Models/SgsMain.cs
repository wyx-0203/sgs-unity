using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
{
    public class SgsMain : Singleton<SgsMain>
    {
        public async Task Init()
        {
            if (Room.Instance.IsSingle && !MCTS.Instance.isRunning) Self.Instance.team = UnityEngine.Random.value < 0.5f ? Team.BLUE : Team.RED;

            // 初始化玩家
            players = Mode.Instance.InitPlayers();
            // if (mode is Mode.四对四)
            // {
            //     players = new Player[8];
            //     for (int i = 0; i < 4; i++) players[i] = new Player(Team.BLUE);
            //     for (int i = 4; i < 8; i++) players[i] = new Player(Team.RED);
            // }
            // else players = new Player[]
            // {
            //     new Player(Team.BLUE),
            //     new Player(Team.RED),
            //     new Player(Team.RED),
            //     new Player(Team.BLUE)
            // };

            AlivePlayers = new List<Player>(players);
            var blueList = new List<Player>();
            var redList = new List<Player>();
            // var enemys = new List<Player>();


            // {
            //     for (int i = 0; i < 4; i++)
            //     {
            //         if (Room.Instance.players[i] == Self.Instance.UserId)
            //         {
            //             players[i].isSelf = true;
            //             Self.Instance.team = players[i].team;
            //         }
            //     }
            // }

            // else
            // {
            foreach (var i in players)
            {
                i.isSelf = i.team == Self.Instance.team;
                i.isAI = Room.Instance.IsSingle && !i.isSelf;

                i.teammates = i.team == Team.BLUE ? blueList : redList;
                i.teammates.Add(i);
            }
            // }

            for (int i = 0; i < players.Length; i++)
            {
                players[i].position = i;
                players[i].next = players[(i + 1) % players.Length];
                players[i].last = players[(i - 1 + players.Length) % players.Length];
            }

            // for (int i = 0; i < 4; i++) players[i].position = i;
            // for (int i = 1; i < 4; i++) players[i].last = players[i - 1];
            // for (int i = 0; i < 4 - 1; i++) players[i].next = players[i + 1];

            // players[3].next = players[0];
            // players[0].last = players[3];

            // PositionView?.Invoke();

            // 初始化武将
            if (!MCTS.Instance.isRunning) await BanPick.Instance.Run();
            foreach (var (i, j) in BanPick.Instance.generals) await i.InitGeneral(j);
            // for (int i = 0; i < players.Length; i++) players[i].InitGeneral(BanPick.Instance.generals[i]);
            AfterBanPickView?.Invoke();

            // 初始化回合
            TurnSystem.Instance.Init();
            // 初始化牌堆
            await CardPile.Instance.Init();
        }

        public async void Run()
        {
            // await DebugCard();
            foreach (var i in players) await new GetCardFromPile(i, 4).Execute();

            // 开始第一个回合
            try { await TurnSystem.Instance.Run(); }
            catch (GameOverException e) { GameOver.Instance.Run(e.loser); }
            catch (FinishSimulation) { MCTS.Instance.state = MCTS.State.Ready; UnityEngine.Debug.Log("end"); }
        }

        // 模式
        // public Mode mode { get; private set; } = Room.Instance.mode;
        // 玩家
        public Player[] players { get; private set; }
        public List<Player> AlivePlayers { get; private set; }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "丈八蛇矛", 
                // "无中生有", "诸葛连弩", "顺手牵羊", "铁索连环", "寒冰剑", "酒"
            };

            var player = players.First(x => x.isSelf);

            while (list.Count > 0)
            {
                var card = await CardPile.Instance.Pop();
                // await new GetCardFromPile(player, 1).Execute();
                // var newCard = player.HandCards[player.HandCardCount - 1];
                if (!list.Contains(card.name)) CardPile.Instance.RemainPile.Add(card);
                else
                {
                    list.Remove(card.name);
                    CardPile.Instance.RemainPile.Insert(0, card);
                    await new GetCardFromPile(player, 1).Execute();
                }
            }
        }

        /// <summary>
        /// 当前最大体力值
        /// </summary>
        public int MaxHp(Player exp = null)
        {
            int maxHp = 0;
            foreach (var i in AlivePlayers)
            {
                if (i != exp && i.Hp > maxHp) maxHp = i.Hp;
            }
            return maxHp;
        }

        /// <summary>
        /// 当前最少手牌数
        /// </summary>
        public int MinHandCard(Player exp = null)
        {
            int minHand = int.MaxValue;
            foreach (var i in AlivePlayers)
            {
                if (i != exp && i.HandCardCount < minHand) minHand = i.HandCardCount;
            }
            return minHand;
        }

        public async Task MoveSeat(Player player)
        {
            if (MCTS.Instance.isRunning) return;
            if (!player.isSelf || player == View.SgsMain.Instance.self.model) return;

            await new Delay(0.5f).Run();
            MoveSeatView?.Invoke(player);
        }

        // public UnityAction PositionView { get; set; }
        public UnityAction AfterBanPickView { get; set; }
        public UnityAction<Player> MoveSeatView { get; set; }
    }
}