using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    /// <summary>
    /// 用于暂停主线程，并获得玩家的操作结果
    /// </summary>
    public class Timer : Singleton<Timer>
    {
        public List<Player> players { get; protected set; } = new();
        public Decision temp { get; protected set; } = new();
        public Func<Decision> DefaultAI { get; set; } = () => new Decision();

        #region  以下属性用于规定玩家的操作方式，如可指定的目标，可选中的牌等

        private int _maxCard;
        private int _minCard;
        private Func<int> _maxDest = () => 0;
        private Func<int> _minDest = () => 0;
        private Predicate<Card> _isValidCard = x => x.discardable;
        private Predicate<Player> _isValidDest = x => true;

        public int maxCard
        {
            get => temp.skill is null ? _maxCard : temp.skill.MaxCard;
            set => _maxCard = value;
        }
        public int minCard
        {
            get => temp.skill is null ? _minCard : temp.skill.MinCard;
            set => _minCard = value;
        }
        public Predicate<Card> isValidCard
        {
            get => temp.skill is null ? _isValidCard : temp.skill.IsValidCard;
            set => _isValidCard = value;
        }
        public Func<int> maxDest
        {
            get => temp.skill is null || temp.skill is Model.Converted ? _maxDest : () => temp.skill.MaxDest;
            set => _maxDest = value;
        }
        public Func<int> minDest
        {
            get => temp.skill is null || temp.skill is Model.Converted ? _minDest : () => temp.skill.MinDest;
            set => _minDest = value;
        }
        public Predicate<Player> isValidDest
        {
            get => temp.skill is null || temp.skill is Model.Converted ? _isValidDest : temp.skill.IsValidDest;
            set => _isValidDest = value;
        }

        public enum Type
        {
            Normal,
            WXKJ,
            Compete,
            PlayPhase
        }

        public Type type { get; set; } = Type.Normal;
        // 可取消，即是否显示取消按钮
        public bool refusable { get; set; } = true;
        // 转换牌列表，如仁德选择一种基本牌
        public List<Card> multiConvert { get; private set; } = new();

        public Equipment equipSkill { get; set; }
        public string hint { get; set; }
        public int second { get; protected set; }

        #endregion
        // int ttt = 0;

        /// <summary>
        /// 暂停主线程，等待玩家传入操作结果
        /// </summary>
        public async Task<Decision> Run(Player player)
        {
            players.Add(player);
            temp.src = player;
            second = minCard > 1 ? 10 + minCard : 15;

            if (player.isSelf && !MCTS.Instance.isRunning)
            {
                await SgsMain.Instance.MoveSeat(player);
            }
            StartTimerView?.Invoke();

            await AutoDecision();

            var decision = await WaitResult();
            // if (this == MCTS.Instance._timer) Debug.Log(888);
            // if (Decision.List.Instance == MCTS.Instance._decisionList) Debug.Log(999);
            // Debug.Log(ttt++);

            if (!decision.action && !refusable)
            {
                decision.action = true;
                if (minCard > 0)
                {
                    // var cards = player.HandCards.Union(player.Equipments.Values).Where(x => isValidCard(x));
                    decision.cards = player.cards.Take(minCard).ToList();
                }
                if (minDest() > 0)
                {
                    var dests = SgsMain.Instance.AlivePlayers.Where(x => isValidDest(x));
                    decision.dests = dests.Take(minDest()).ToList();
                }
            }

            Reset();

            if (decision.skill is Converted converted)
            {
                decision.cards = new List<Card> { converted.Use(decision.cards) };
            }

            return decision;
        }

        public async Task<Decision> Run(Player player, int cardCount, int destCount)
        {
            this.maxCard = cardCount;
            this.minCard = cardCount;

            if (destCount > 0)
            {
                this.maxDest = () => destCount;
                this.minDest = () => destCount;
            }

            return await Run(player);
        }

        protected async Task<Decision> WaitResult()
        {
            if (!Room.Instance.IsSingle)
            {
                var message = await WebSocket.Instance.PopMessage();
                var json = JsonUtility.FromJson<Decision.Message>(message);

                Decision.List.Instance.Push(json);
            }

            // var d = await Decision.Pop();
            // Util.Print("hint=" + hint + "\ndecision=\n" + d);
            // return d;
            return await Decision.List.Instance.Pop();
        }

        public void SendDecision(Decision decision = null)
        {
            // Util.Print(Decision.List.Instance);
            if (decision is null)
            {
                decision = temp;
                // temp = new Decision();
            }
            Delay.StopAll();

            // if (!decision.action)
            // {
            //     decision.cards.Clear();
            //     decision.dests.Clear();
            //     decision.skill = null;
            //     decision.converted = null;
            // }

            if (Room.Instance.IsSingle) Decision.List.Instance.Push(decision);
            else
            {
                var json = decision.ToMessage();
                if (decision.src != null) json.src = decision.src.position;

                WebSocket.Instance.SendMessage(json);
            }
            // Util.Print(Decision.List.Instance);
            // Util.Print("4");
        }

        public Decision SaveTemp()
        {
            var t = temp;
            temp = new Decision();
            return t;
        }

        private void Reset()
        {
            temp = new Decision();
            StopTimerView?.Invoke();

            players.Clear();
            hint = "";
            maxCard = 0;
            minCard = 0;
            maxDest = () => 0;
            minDest = () => 0;
            isValidCard = card => card.discardable;
            isValidDest = dest => true;
            equipSkill = null;
            type = Type.Normal;
            refusable = true;
            multiConvert.Clear();
            DefaultAI = () => new Decision();
        }

        protected async Task AutoDecision()
        {
            Decision decision = null;
            switch (MCTS.Instance.state)
            {
                case MCTS.State.Disable:
                    if (players[0].isSelf)
                    {
                        if (!await new Delay(second).Run()) return;
                        decision = new Decision();
                    }
                    else if (players[0].isAI)
                    {
                        await new Delay(1f).Run();
                        decision = DefaultAI();
                    }
                    break;
                case MCTS.State.Ready:
                    if (players[0].isSelf)
                    {
                        if (!await new Delay(second).Run()) return;
                        decision = new Decision();
                    }
                    else if (players[0].isAI)
                    {
                        await new Delay(1f).Run();
                        decision = await MCTS.Instance.Run(MCTS.State.WaitTimer);
                    }
                    break;
                case MCTS.State.Restoring:
                    if (Decision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitTimer;
                    return;
                case MCTS.State.Simulating:
                    decision = DefaultAI();
                    break;
            }

            SendDecision(decision);
        }

        public async Task<Decision> RunWxkj(Card scheme, Team team)
        {
            players.AddRange(team.GetAllPlayers());
            hint = scheme + "即将对" + scheme.dest + "生效，是否使用无懈可击？";
            maxCard = 1;
            minCard = 1;
            isValidCard = x => x is 无懈可击;
            type = Type.WXKJ;
            DefaultAI = () =>
            {
                foreach (var i in players)
                {
                    var card = i.FindCard<无懈可击>();
                    if (card is null || scheme.Src.team == team) continue;

                    return new Decision { src = i, action = true, cards = new List<Card> { card } };
                }
                return new();
            };

            StartTimerView?.Invoke();
            await AutoDecision();
            var decision = await WaitResult();
            Reset();
            return decision;
        }

        // public async Task<Decision> RunCompete(Player player0,Player player1)
        // {
        //     players.Add(player0);
        //     players.Add(player1);
        //     hint = "请选择一张手牌拼点";
        //     maxCard = 1;
        //     minCard = 1;
        //     isValidCard = x => x.IsHandCard;
        //     type = Type.Compete;
        //     DefaultAI = () =>

        //     StartTimerView?.Invoke();
        //     await AutoDecision();
        //     var decision = await WaitResult();
        //     Reset();
        //     return decision;
        // }


        public UnityAction StartTimerView { get; set; }
        public UnityAction StopTimerView { get; set; }

        protected static Timer currentInstance;
        public static new Timer Instance => currentInstance is null ? Singleton<Timer>.Instance : currentInstance;
    }
}