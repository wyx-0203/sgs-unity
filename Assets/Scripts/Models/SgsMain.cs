using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace Model
{
    public class SgsMain : Singleton<SgsMain>
    {
        public async void Run()
        {
            // 初始化玩家

            AlivePlayers = new List<Player>(players);

            if (!Room.Instance.IsSingle)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Room.Instance.players[i] == Self.Instance.UserId)
                    {
                        players[i].isSelf = true;
                        Self.Instance.team = players[i].team;
                    }
                }
            }

            else if (mode is Mode.统帅双军)
            {
                Self.Instance.team = UnityEngine.Random.value < 0.5f;
                foreach (var i in players)
                {
                    i.isSelf = i.team == Self.Instance.team;
                    i.isAI = !i.isSelf;
                }
            }

            players[0].teammate = players[3];
            players[1].teammate = players[2];
            players[2].teammate = players[1];
            players[3].teammate = players[0];

            for (int i = 0; i < 4; i++) players[i].position = i;
            for (int i = 1; i < 4; i++) players[i].last = players[i - 1];
            for (int i = 0; i < 4 - 1; i++) players[i].next = players[i + 1];

            players[3].next = players[0];
            players[0].last = players[3];

            await Task.Yield();
            PositionView?.Invoke(players);

            // 初始化武将
            await BanPick.Instance.Run();
            GeneralView?.Invoke();

            // 初始化牌堆
            TurnSystem.Instance.Init();
            await CardPile.Instance.Init();

            foreach (var i in players) await new GetCardFromPile(i, 4).Execute();

            // 开始第一个回合
            await TurnSystem.Instance.Run();

            GameOver.Instance.Run();
        }


        // 模式
        public Mode mode { get; private set; } = Room.Instance.mode;
        // 玩家
        public Player[] players { get; private set; } = new Player[]
        {
            new Player(Team.BLUE),
            new Player(Team.RED),
            new Player(Team.RED),
            new Player(Team.BLUE),
        };
        public List<Player> AlivePlayers { get; private set; }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "火杀", "无中生有", "诸葛连弩", "顺手牵羊", "铁索连环", "寒冰剑", "酒"
            };

            while (list.Count > 0)
            {
                await new GetCardFromPile(players[0], 1).Execute();
                var newCard = players[0].HandCards[players[0].HandCardCount - 1];
                if (!list.Contains(newCard.Name)) await new Discard(players[0], new List<Card> { newCard }).Execute();
                else list.Remove(newCard.Name);
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
        public int MinHand(Player exp = null)
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
            if (mode is not Mode.统帅双军) return;
            if (!player.isSelf || player == View.SgsMain.Instance.self.model) return;

            await new Delay(0.5f).Run();
            MoveSeatView?.Invoke(player);
        }

        public UnityAction<Player[]> PositionView { get; set; }
        public UnityAction GeneralView { get; set; }
        public UnityAction<Player> MoveSeatView { get; set; }
        public UnityAction<ChatMessage> ChatView { get; set; }
    }
}