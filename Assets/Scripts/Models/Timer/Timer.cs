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

        public Player player { get; protected set; }
        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public Func<int> MaxDest { get; set; }
        public Func<int> MinDest { get; set; }
        public Func<Card, bool> IsValidCard { get; set; } = card => !card.IsConvert;
        public Func<Player, bool> IsValidDest { get; set; } = dest => true;

        // 是否处于出牌阶段，此属性用于控制出牌阶段技能
        public bool isPerformPhase { get; set; } = false;
        // 无懈可击
        public bool isWxkj { get; protected set; } = false;
        // 拼点
        public bool isCompete { get; private set; } = false;
        // 可取消，即控制取消按钮
        public bool Refusable { get; set; } = true;
        // 转换牌列表，如仁德选择一种基本牌
        public List<Card> MultiConvert { get; set; } = new List<Card>();

        public string Hint { get; set; }
        public string GivenSkill { get; set; } = "";

        public int second;

        // 以下属性用于保存操作结果

        public List<Card> Cards { get; set; }
        public List<Player> Dests { get; set; }
        public string Skill { get; set; }
        public string Other { get; set; }

        /// <summary>
        /// 暂停主线程，等待玩家传入操作结果
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player)
        {
            this.player = player;
            second = minCard > 1 ? 10 + minCard : 15;

            Cards = new List<Card>();
            Dests = new List<Player>();
            Skill = "";

            if (GameOver.Instance.Check()) return false;

            if (player.isSelf)
            {
                await SgsMain.Instance.MoveSeat(player);
                SelfAutoResult();
            }
            else if (Room.Instance.IsSingle) AIAutoResult();
            // Operation.Instance.Clear();
            StartTimerView?.Invoke();
            bool result = await WaitResult();

            StopTimerView?.Invoke();

            Operation.Instance.Clear();
            if (Room.Instance.IsSingle && player.isAI) Operation.Instance.CopyTimer();

            // 重置
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

            Skill s = player.FindSkill(Skill);
            if (Skill == "丈八蛇矛" || Skill != "" && s is Converted)
            {
                var skill = Skill == "丈八蛇矛" ? (player.weapon as 丈八蛇矛).skill : s as Converted;
                Cards = new List<Card> { skill.Execute(Cards) };
                skill.Execute();
                Skill = "";
            }

            return result;
        }

        public async Task<bool> Run(Player player, int cardCount, int destCount)
        {
            return await Run(player, cardCount, cardCount, destCount, destCount);
        }

        public async Task<bool> Run(Player player, int maxCard, int minCard, int maxDest, int minDest)
        {
            this.maxCard = maxCard;
            this.minCard = minCard;
            if (maxDest > 0) this.MaxDest = () => maxDest;
            if (minDest > 0) this.MinDest = () => minDest;
            return await Run(player);
        }

        private void SetResult(List<int> cards, List<int> dests, string skill, string other)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);
            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);
            Skill = skill;
            Other = other;
        }

        public void SendResult(List<int> cards, List<int> dests, string skill, string other, bool result = true)
        {
            // StopAllCoroutines();
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
            else WS.Instance.SendJson(json);
        }

        public void SendResult()
        {
            SendResult(null, null, null, null, false);
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
                var message = await WS.Instance.PopMsg();
                json = JsonUtility.FromJson<TimerMessage>(message);
            }

            // if (SgsMain.Instance.GameIsOver) return false;

            if (isWxkj)
            {
                if (json.result)
                {
                    player = SgsMain.Instance.players[json.src];
                    SetResult(json.cards, new List<int>(), "", "");
                }
                else
                {
                    wxkjDone++;
                    if (wxkjDone < SgsMain.Instance.AlivePlayers.Count)
                    {
                        // Wss.Instance.Count--;
                        return await WaitResult();
                    }
                }
            }
            else if (isCompete)
            {
                if (json.cards is null) json.cards = new List<int>();
                if (json.src == player0.position) card0 = json.cards.Count > 0 ? json.cards[0] : player0.HandCards[0].Id;
                else card1 = json.cards.Count > 0 ? json.cards[0] : player1.HandCards[0].Id;
                if (card0 == 0 || card1 == 0)
                {
                    // Wss.Instance.Count--;
                    await WaitResult();
                }
            }
            else if (json.result) SetResult(json.cards, json.dests, json.skill, json.other);

            // StopAllCoroutines();
            Delay.StopAll();
            // Util.Instance.StopDelay();
            return json.result;
        }


        private int wxkjDone;
        public async Task<bool> RunWxkj()
        {
            maxCard = 1;
            minCard = 1;
            isWxkj = true;
            IsValidCard = card => card is 无懈可击;

            Cards = new List<Card>();
            Dests = new List<Player>();

            wxkjDone = 0;

            StartTimerView?.Invoke();
            WxkjAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            bool result = await WaitResult();

            StopTimerView?.Invoke();

            maxCard = 0;
            minCard = 0;
            isWxkj = false;
            IsValidCard = card => !card.IsConvert;

            return result;
        }

        public void SendResult(int src, bool result, List<int> cards = null, string skill = "")
        {
            var json = new TimerMessage
            {
                msg_type = "set_result",
                result = result,
                cards = cards,
                src = src,
            };
            // json.eventname = "set_result";
            // json.id = Wss.Instance.Count + 1;
            // json.result = result;
            // json.cards = cards;
            // json.src = src;

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else
            {
                Delay.StopAll();
                // Util.Instance.StopDelay();
                WS.Instance.SendJson(json);
            }
        }

        public Player player0;
        public Player player1;
        public int card0;
        public int card1;
        public async Task Compete(Player player0, Player player1)
        {
            maxCard = 1;
            minCard = 1;
            isCompete = true;
            Refusable = false;
            IsValidCard = card => player0.HandCards.Contains(card) || player1.HandCards.Contains(card);
            this.player0 = player0;
            this.player1 = player1;
            card0 = 0;
            card1 = 0;

            Cards = new List<Card>(2);
            Dests = new List<Player>();

            await SgsMain.Instance.MoveSeat(player0);
            await SgsMain.Instance.MoveSeat(player1);
            // else if (player1.isSelf && player1 != View.SgsMain.Instance.self.model)
            // {
            //     await new Delay(0.5f).Run();
            //     SgsMain.Instance.MoveSeatView(player1);
            // }

            // if (player0.isSelf) moveSeat(player0);
            // else if (player1.isSelf) moveSeat(player1);

            StartTimerView();
            CompeteAutoResult();
            if (Room.Instance.IsSingle) AIAutoResult();
            await WaitResult();

            StopTimerView();

            maxCard = 0;
            minCard = 0;
            isCompete = false;
            Refusable = true;
            IsValidCard = card => !card.IsConvert;
        }

        private async void AIAutoResult()
        {
            // yield return new WaitForSeconds(1);
            if (!await new Delay(1).Run()) return;

            if (isWxkj)
            {
                foreach (var i in SgsMain.Instance.AlivePlayers)
                {
                    if (i.isAI) SendResult(i.position, false, null, "");
                }
            }
            else if (isCompete)
            {
                if (player0.isAI) SendResult(player0.position, false);
                if (player1.isAI) SendResult(player1.position, false);
            }
            else SendResult();
        }

        private async void SelfAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            SendResult();
        }

        private async void WxkjAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i.isSelf) SendResult(i.position, false);
            }
        }

        private async void CompeteAutoResult()
        {
            if (!await new Delay(second).Run()) return;

            if (card0 == 0) SendResult(player0.position, false);
            if (card1 == 0) SendResult(player1.position, false);
        }

        public UnityAction StartTimerView { get; set; }
        public UnityAction StopTimerView { get; set; }
    }
}