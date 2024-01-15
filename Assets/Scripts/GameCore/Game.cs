using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace GameCore
{
    public class Game : Singleton<Game>
    {
        // 玩家
        public Player[] players { get; private set; }
        public List<Player> AlivePlayers { get; private set; }
        // public Team loser { get; private set; }

        public async Task Init()
        {

            // if(hotUpdateAss is null) Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
            //             #if !UNITY_EDITOR
            //         Assembly hotUpdateAss = Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
            // #else
            //       // Editor下无需加载，直接查找获得HotUpdate程序集
            // #endif

            // if (Room.Instance.IsSingle && !MCTS.Instance.isRunning) Self.Instance.team = UnityEngine.Random.value < 0.5f ? Team.BLUE : Team.RED;

            // 初始化玩家
            players = Mode.Instance.InitPlayers();

            AlivePlayers = new List<Player>(players);
            List<Player> blueList = new();
            List<Player> redList = new();

            foreach (var i in players)
            {
                // i.isSelf = i.team == Self.Instance.team;
                i.isAI = Room.Instance.IsSingle && i.team == Room.Instance.AITeam;

                i.teammates = i.team == Team.BLUE ? blueList : redList;
                i.teammates.Add(i);
            }

            for (int i = 0; i < players.Length; i++)
            {
                players[i].position = i;
                players[i].next = players[(i + 1) % players.Length];
                players[i].last = players[(i - 1 + players.Length) % players.Length];
            }

            EventSystem.Instance.Send(new InitPlayer
            {
                id = new int[] { 0, 1 },
                team = players.Select(x => x.team).ToList(),
                position = players.Select(x => x.turnOrder).ToList(),
                isMonarch = players.Select(x => x.isMonarch).ToList()
            });

            // 初始化武将
            // if (!MCTS.Instance.isRunning) 
            await BanPick.Instance.Run();
            foreach (var (player, general) in BanPick.Instance.generals) await player.InitGeneral(general);
            // AfterBanPickView?.Invoke();

            // 初始化回合
            TurnSystem.Instance.Init();
            // 初始化牌堆
            await CardPile.Instance.Init();
        }

        public async void Run()
        {
            await DebugCard();
            foreach (var i in players) await new DrawCard(i, 4).Execute();

            // 开始第一个回合
            try { await TurnSystem.Instance.Run(); }
            catch (GameOverException e)
            {
                // loser = e.loser;
                // GameOverView?.Invoke();
                EventSystem.Instance.Send(new Model.GameOver { loser = e.loser });
            }
            // catch (FinishSimulation) { MCTS.Instance.state = MCTS.State.Ready; UnityEngine.Debug.Log("end"); }
        }

        // public void SendSurrender()
        // {
        //     var message = new Decision.Message(Self.Instance.team);
        //     if (Room.Instance.IsSingle) Decision.List.Instance.Push(message);
        //     else WebSocket.Instance.SendMessage(message);
        // }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "丈八蛇矛", 
                // "无中生有", "诸葛连弩", "顺手牵羊", "铁索连环", "寒冰剑", "酒"
            };

            var player = players.First(x => x.turnOrder == 0);

            while (list.Count > 0)
            {
                var card = await CardPile.Instance.Pop();
                // await new GetCardFromPile(player, 1).Execute();
                // var newCard = player.HandCards[player.HandCardCount - 1];
                if (!list.Contains(card.name)) CardPile.Instance.AddToDiscard(card, null);
                else
                {
                    list.Remove(card.name);
                    CardPile.Instance.RemainPile.Insert(0, card);
                    await new DrawCard(player, 1).Execute();
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
                if (i != exp && i.hp > maxHp) maxHp = i.hp;
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
                if (i != exp && i.handCardsCount < minHand) minHand = i.handCardsCount;
            }
            return minHand;
        }

        // public async Task MoveSeat(Player player)
        // {
        //     if (MCTS.Instance.isRunning || !player.isSelf || player == currentSeat) return;
        //     currentSeat = player;
        //     // if (!player.isSelf || player == View.SgsMain.Instance.self.model) return;

        //     await new Delay(0.5f).Run();
        //     MoveSeatView?.Invoke(player);
        // }
        // private Player currentSeat;

        // public Action AfterBanPickView;
        // public Action<Player> MoveSeatView;
        // public Action GameOverView;
    }
}