using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Model
{
    /// <summary>
    /// 用于暂停主线程，并获得玩家的操作结果
    /// </summary>
    public class Timer : Singleton<Timer>
    {
        public List<Player> players { get; protected set; } = new();


        #region  以下属性用于规定玩家的操作方式，如可指定的目标，可选中的牌等

        private int _maxCard;
        private int _minCard;
        private Func<int> _maxDest;
        private Func<int> _minDest;
        private Func<Card, bool> _isValidCard;
        private Func<Player, bool> _isValidDest;

        // 是否处于出牌阶段，此属性用于控制出牌阶段技能
        public bool isPlayPhase { get; set; } = false;
        // 可取消，即是否显示取消按钮
        public bool refusable { get; set; } = true;
        // 转换牌列表，如仁德选择一种基本牌
        public List<Card> multiConvert { get; private set; } = new();

        public string givenSkill { get; set; }
        public string hint { get; set; }
        public int second { get; protected set; }

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
        public Func<Card, bool> isValidCard
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
        public Func<Player, bool> isValidDest
        {
            get => temp.skill is null || temp.skill is Model.Converted ? _isValidDest : temp.skill.IsValidDest;
            set => _isValidDest = value;
        }

        #endregion

        public Decision temp { get; private set; } = new();

        public Func<Decision> AIDecision { get; set; } = () => new Decision();

        /// <summary>
        /// 暂停主线程，等待玩家传入操作结果
        /// </summary>
        public async Task<Decision> Run(Player player)
        {
            players.Clear();
            players.Add(player);
            second = minCard > 1 ? 10 + minCard : 15;

            if (player.isSelf)
            {
                await SgsMain.Instance.MoveSeat(player);
                SelfAutoResult();
            }
            else if (Room.Instance.IsSingle) AIAutoResult();
            StartTimerView?.Invoke();
            var decision = await WaitResult();

            StopTimerView?.Invoke();

            if (!decision.action && !refusable)
            {
                decision.action = true;
                if (minCard > 0)
                {
                    var cards = player.HandCards.Union(player.Equipments.Values).Where(x => isValidCard(x));
                    decision.cards = cards.ToList().GetRange(0, minCard);
                }
                if (minDest() > 0)
                {
                    var dests = SgsMain.Instance.AlivePlayers.Where(x => isValidDest(x));
                    decision.dests = dests.ToList().GetRange(0, minDest());
                }
            }

            Reset();

            if (decision.skill is Converted converted)
            {
                decision = new Decision
                {
                    action = true,
                    cards = new List<Card> { converted.Convert(decision.cards) },
                    dests = decision.dests,
                };
                await converted.Execute(decision);
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

        private async Task<Decision> WaitResult()
        {
            if (!Room.Instance.IsSingle)
            {
                var message = await WebSocket.Instance.PopMessage();
                var json = JsonUtility.FromJson<TimerMessage>(message);

                Decision.list.Add(new Decision
                {
                    action = json.action,
                    cards = json.cards.Select(x => CardPile.Instance.cards[x]).ToList(),
                    dests = json.dests.Select(x => SgsMain.Instance.players[x]).ToList(),
                    skill = players[0].FindSkill(json.skill),
                    converted = multiConvert.Find(x => x.name == json.other)
                });
            }

            var decision = await Decision.Pop();
            Debug.Log(2);
            Delay.StopAll();
            return decision;
        }

        public void SendDecision(Decision decision = null)
        {
            if (decision is null)
            {
                decision = temp;
                temp = new Decision();
            }

            if (!decision.action)
            {
                decision.cards.Clear();
                decision.dests.Clear();
                decision.skill = null;
                decision.converted = null;
            }

            if (Room.Instance.IsSingle) Decision.list.Add(decision);
            else
            {
                var json = new TimerMessage
                {
                    msg_type = "set_result",
                    action = decision.action,
                    cards = decision.cards.Select(x => x.id).ToList(),
                    dests = decision.dests.Select(x => x.position).ToList(),
                    skill = decision.skill?.Name,
                    other = decision.converted?.name,
                };
                if (decision.src != null) json.src = decision.src.position;

                WebSocket.Instance.SendMessage(json);
            }
        }

        public Decision SaveTemp()
        {
            var t = temp;
            temp = new Decision();
            return t;
        }

        private void Reset()
        {
            hint = "";
            maxCard = 0;
            minCard = 0;
            maxDest = () => 0;
            minDest = () => 0;
            isValidCard = card => !card.IsConvert;
            isValidDest = dest => true;
            givenSkill = "";
            isPlayPhase = false;
            refusable = true;
            multiConvert.Clear();
            AIDecision = () => new Decision();
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1f).Run()) return;
            if (givenSkill != "" && players[0].FindSkill(givenSkill) is Triggered triggered)
            {
                temp.skill = triggered;
            }

            SendDecision(AIDecision());
            Debug.Log(1);
            temp.skill = null;

        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;
            SendDecision();
        }

        public UnityAction StartTimerView { get; set; }
        public UnityAction StopTimerView { get; set; }

        protected static Timer currentInstance;
        public static new Timer Instance => currentInstance is null ? Singleton<Timer>.Instance : currentInstance;
    }
}