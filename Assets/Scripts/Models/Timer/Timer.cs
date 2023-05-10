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
        // 单机模式中使用tcs阻塞线程
        protected TaskCompletionSource<TimerMessage> waitAction;

        // 以下属性用于规定玩家的操作方式，如可指定的目标，可选中的牌等

        public List<Player> players { get; protected set; } = new List<Player>();
        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public Func<int> MaxDest { get; set; }
        public Func<int> MinDest { get; set; }
        public Func<Card, bool> IsValidCard { get; set; } = card => !card.IsConvert;
        public Func<Player, bool> IsValidDest { get; set; } = dest => true;

        // 是否处于出牌阶段，此属性用于控制出牌阶段技能
        public bool isPerformPhase { get; set; } = false;
        // 可取消，即是否显示取消按钮
        public bool Refusable { get; set; } = true;
        // 转换牌列表，如仁德选择一种基本牌
        public List<Card> MultiConvert { get; set; } = new List<Card>();

        public string GivenSkill { get; set; } = "";
        public string Hint { get; set; }
        public int second { get; protected set; }

        // 以下属性用于保存操作结果

        public List<Card> cards { get; set; }
        public List<Player> dests { get; set; }
        public Skill skill { get; set; }
        public string other { get; set; }

        /// <summary>
        /// 暂停主线程，等待玩家传入操作结果
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player)
        {
            players.Clear();
            players.Add(player);
            second = minCard > 1 ? 10 + minCard : 15;

            if (GameOver.Instance.Check()) return false;

            if (player.isSelf)
            {
                await SgsMain.Instance.MoveSeat(player);
                SelfAutoResult();
            }
            else if (Room.Instance.IsSingle) AIAutoResult();
            StartTimerView?.Invoke();
            bool result = await WaitResult();

            StopTimerView?.Invoke();

            Operation.Instance.Clear();
            if (Room.Instance.IsSingle && player.isAI) Operation.Instance.SetAITimer();

            Reset();

            if (skill is Converted)
            {
                cards = new List<Card> { (skill as Converted).Execute(cards) };
                skill.Execute();
                skill = null;
            }

            return result;
        }

        public async Task<bool> Run(Player player, int cardCount, int destCount)
        {
            this.maxCard = cardCount;
            this.minCard = cardCount;

            if (destCount > 0)
            {
                this.MaxDest = () => destCount;
                this.MinDest = () => destCount;
            }

            return await Run(player);
        }

        private async Task<bool> WaitResult()
        {
            TimerMessage json;
            if (Room.Instance.IsSingle)
            {
                // 若为单机模式，则通过tcs阻塞线程，等待操作结果
                waitAction = new TaskCompletionSource<TimerMessage>();
                json = await waitAction.Task;
            }
            else
            {
                // 若为多人模式，则等待ws通道传入消息
                var message = await WebSocket.Instance.PopMessage();
                json = JsonUtility.FromJson<TimerMessage>(message);
            }

            if (json.result)
            {
                cards = json.cards.Select(x => CardPile.Instance.cards[x]).ToList();
                dests = json.dests.Select(x => SgsMain.Instance.players[x]).ToList();
                skill = players[0].FindSkill(json.skill);
                other = json.other;
            }
            else
            {
                cards = null;
                dests = null;
                skill = null;
                other = "";
            }

            return json.result;
        }

        public void SendResult(List<int> cards, List<int> dests, string skill, string other, bool result = true)
        {
            Delay.StopAll();
            var json = new TimerMessage
            {
                msg_type = "set_result",
                result = result,
                cards = cards,
                dests = dests,
                skill = skill,
                other = other,
            };

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else WebSocket.Instance.SendMessage(json);
        }

        public void SendResult()
        {
            SendResult(null, null, null, null, false);
        }

        private void Reset()
        {
            Hint = "";
            maxCard = 0;
            minCard = 0;
            MaxDest = () => 0;
            MinDest = () => 0;
            IsValidCard = card => !card.IsConvert;
            IsValidDest = dest => true;
            GivenSkill = "";
            Refusable = true;
            MultiConvert.Clear();
        }

        private async void AIAutoResult()
        {
            if (!await new Delay(1).Run()) return;
            SendResult();
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;
            SendResult();
        }

        public UnityAction StartTimerView { get; set; }
        public UnityAction StopTimerView { get; set; }

        protected static Timer currentInstance;
        public static new Timer Instance => currentInstance is null ? Singleton<Timer>.Instance : currentInstance;
    }
}