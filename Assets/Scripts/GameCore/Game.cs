using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mode = Model.Mode;
using Team = Model.Team;

namespace GameCore
{
    public class Game
    {
        // 玩家
        public Player[] players { get; private set; }
        public List<Player> AlivePlayers { get; private set; }
        public Mode mode;
        public EventSystem eventSystem;
        public BanPick banPick;
        public CardPile cardPile;
        public TurnSystem turnSystem;
        public AI ai;
        private List<int> users;

        public Game(Mode mode, List<int> users, Action<Model.Message> onSendMessage)
        {
            eventSystem = new EventSystem(onSendMessage);
            ai = new AI(this);
            this.mode = mode;
            this.users = users;

            // 初始化玩家
            players = mode.InitPlayers(this);

            AlivePlayers = new List<Player>(players);
            List<Player> blueList = new();
            List<Player> redList = new();

            foreach (var i in players)
            {
                i.game = this;
                i.teammates = i.team == Team.BLUE ? blueList : redList;
                i.teammates.Add(i);
            }

            for (int i = 0; i < players.Length; i++)
            {
                players[i].position = i;
                players[i].next = players[(i + 1) % players.Length];
                players[i].last = players[(i - 1 + players.Length) % players.Length];
            }
        }

        public Game(Mode mode, List<int> users, Action<Model.Message> onSendMessage, Team aiTeam) : this(mode, users, onSendMessage)
        {
            foreach (var i in players) i.isAI = i.team == aiTeam;
        }

        public async Task Init()
        {
            eventSystem.SendToClient(new Model.InitPlayer
            {
                id = users.ToList(),
                team = players.Select(x => x.team).ToList(),
                position = players.Select(x => x.turnOrder).ToList(),
                isMonarch = players.Select(x => x.isMonarch).ToList()
            });

            // 初始化武将
            // if (!MCTS.Instance.isRunning) 
            banPick = new BanPick(this);
            await banPick.Run();
            foreach (var (player, general) in banPick.generals) player.InitGeneral(general);

            // 初始化回合
            turnSystem = new TurnSystem(this);
            // 初始化牌堆
            cardPile = new CardPile(this);
        }

        public async Task Run()
        {
            await DebugCard();
            foreach (var i in players) await new DrawCard(i, 4).Execute();

            // 开始第一个回合
            try { await turnSystem.Run(); }
            catch (GameOverException e)
            {
                eventSystem.SendToClient(new Model.GameOver { loser = e.loser });
            }
            catch (Exception)
            {
                eventSystem.SendToClient(new Model.GameOver { loser = Team.None });
            }
        }
        private async Task DebugCard()
        {
            var list = new List<string>
            {
                // "寒冰剑", 
                // "无中生有", "诸葛连弩", "顺手牵羊", "铁索连环", "寒冰剑", "酒"
            };

            var player = players.First(x => x.turnOrder == 0);

            while (list.Count > 0)
            {
                var card = await cardPile.Pop();
                if (!list.Contains(card.name)) cardPile.AddToDiscard(card, null);
                else
                {
                    list.Remove(card.name);
                    cardPile.RemainPile.Insert(0, card);
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
    }
}