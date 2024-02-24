using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Type = Model.SinglePlayQuery.Type;

namespace GameCore
{
    /// <summary>
    /// 出牌请求
    /// </summary>
    public class PlayQuery
    {
        /// <summary>
        /// 出牌玩家
        /// </summary>
        public Player player;

        private Game game => player.game;
        /// <summary>
        /// 出牌类型
        /// </summary>
        public Type type { get; set; } = Type.Normal;
        /// <summary>
        /// 可取消，即是否显示取消按钮
        /// </summary>
        public bool refusable { get; set; } = true;
        /// <summary>
        /// 出牌提示
        /// </summary>
        public string hint { get; set; }
        /// <summary>
        /// 时间限制
        /// </summary>
        public int second { get; set; } = 15;
        /// <summary>
        /// 技能名
        /// </summary>
        public string skill { get; set; }

        /// <summary>
        /// 最多可选牌数
        /// </summary>
        public int maxCard { get; set; }
        /// <summary>
        /// 最少可选牌数
        /// </summary>
        public int minCard { get; set; }
        /// <summary>
        /// 每张牌是否可选
        /// </summary>
        public Predicate<Card> isValidCard = x => x.discardable;

        /// <summary>
        /// 最多可选目标数
        /// </summary>
        public int maxDest { get; set; }
        /// <summary>
        /// 最少可选目标数
        /// </summary>
        public int minDest { get; set; }
        /// <summary>
        /// 每个目标是否可选
        /// </summary>
        public Func<Player, bool> isValidDest;
        /// <summary>
        /// 是否可成为第二个目标 (明策，眩惑)
        /// </summary>
        public Func<Player, Player, bool> isValidSecondDest;

        public Func<Card, int> maxDestForCard;
        public Func<Card, int> minDestForCard;
        public Func<Player, Card, bool> isValidDestForCard;
        public bool diffDest => maxDestForCard != null && minDestForCard != null && isValidDestForCard != null;

        /// <summary>
        /// 虚拟牌列表，如仁德选择一种基本牌
        /// </summary>
        public List<Card> virtualCards { get; set; } = new();
        /// <summary>
        /// 虚拟牌是否可选
        /// </summary>
        public Predicate<Card> isValidVirtualCard = x => true;

        // public PlayQuery

        /// <summary>
        /// AI行动时的决策
        /// </summary>
        public Func<PlayDecision> defaultAI;
        /// <summary>
        /// AI是否行动
        /// </summary>
        public bool aiAct = true;

        public IEnumerable<PlayQuery> skillQuerys { get; private set; }

        private Model.SinglePlayQuery ToMessage()
        {
            var players = game.AlivePlayers;

            // 初始化基本信息
            var message = new Model.SinglePlayQuery
            {
                type = type,
                hint = hint,
                skillName = skill,

                maxCard = maxCard,
                minCard = minCard,
            };

            // 初始化虚拟牌
            foreach (var i in virtualCards)
            {
                if (isValidVirtualCard(i)) message.virtualCards.Add(i.id);
                else message.disabledVirtualCards.Add(i.id);
            }

            // 初始化目标信息
            if (!diffDest)
            {
                var firsts = players.Where(x => maxDest > 0 && isValidDest(x));
                message.destInfos.Add(new Model.SinglePlayQuery.DestInfo
                {
                    cards = maxCard > 0 ? player.cards.Where(x => isValidCard(x)).Select(x => x.id).ToList() : new(),
                    maxDest = maxDest,
                    minDest = minDest,
                    dests = firsts.Select(x => x.position).ToList(),
                    secondDests = isValidSecondDest != null ? firsts.Select(first => players
                        .Where(x => isValidSecondDest(x, first))
                        .Select(x => x.position)
                        .ToList()).ToList() : new()
                });
            }
            else
            {
                var cards = virtualCards.Count == 0
                    ? player.cards.Where(x => isValidCard(x))
                    : virtualCards.Where(x => isValidVirtualCard(x));

                foreach (var i in cards)
                {
                    int maxd = maxDestForCard(i);
                    int mind = minDestForCard(i);
                    var ds = players.Where(x => isValidDestForCard(x, i));

                    var destInfo = message.destInfos.Find(x => x.maxDest == maxd
                        && x.minDest == mind
                        && x.dests.SequenceEqual(ds.Select(y => y.position)));

                    if (destInfo != null)
                    {
                        destInfo.cards.Add(i.id);
                    }
                    else
                    {
                        destInfo = new Model.SinglePlayQuery.DestInfo
                        {
                            cards = new List<int> { i.id },
                            maxDest = maxd,
                            minDest = mind,
                            dests = ds.Select(x => x.position).ToList()
                        };
                        message.destInfos.Add(destInfo);
                    }
                }
            }
            return message;
        }

        public async Task<PlayDecision> Run()
        {
            // 添加可选技能
            skillQuerys = player.skills.Where(x => (x is Active && type == Type.PlayPhase // 主动技
                || x is Converted converted1 && isValidCard(converted1.Convert(null))) // 转化技
                && x.IsValid).Select(x => x.ToPlayQuery(this));
            // 初始化出牌消息
            // var message = ToMessage();
            var message = new Model.PlayQuery
            {
                player = player.position,
                second = second,
                refusable = refusable,
                origin = ToMessage(),
                skills = skillQuerys.Select(x => x.ToMessage()).ToList()
            };
            // message.skills = player.skills.Where(x => (x is Active && type == Type.PlayPhase // 主动技
            //     || x is Converted converted1 && isValidCard(converted1.Convert(null))) // 转化技
            //     && x.IsValid).Select(x => x.ToPlayRequest(this).ToMessage() as Model.SinglePlayQuery).ToList();
            // 发送开始出牌消息
            // string a = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            // UnityEngine.Debug.Log(a);
            // var b = Newtonsoft.Json.JsonConvert.DeserializeObject(a, typeof(Model.PlayQuery));
            // a = Newtonsoft.Json.JsonConvert.SerializeObject(b);
            // UnityEngine.Debug.Log(a);
            game.eventSystem.SendToClient(message);

            // 自动出牌
            var cts = new CancellationTokenSource();
            AutoDecision(cts.Token);

            // 等待玩家或AI决策
            var decision = await WaitDecision();
            cts.Cancel();
            cts.Dispose();

            // 发送完成出牌消息
            game.eventSystem.SendToClient(new Model.FinishPlay
            {
                player = player.position,
                type = type
            });

            if (!decision.action)
            {
                // 若本次出牌不可取消，则强制选择
                if (!refusable)
                {
                    decision.action = true;
                    if (minCard > 0)
                    {
                        decision.cards = player.cards
                            .Where(x => isValidCard(x))
                            .Take(minCard)
                            .ToList();
                    }
                    if (minDest > 0)
                    {
                        decision.dests = game.AlivePlayers
                            .Where(x => isValidDest(x))
                            .Take(minDest)
                            .ToList();
                    }
                }
                else return decision;
            }

            // 转化技
            if (decision.skill is Converted converted)
            {
                decision.cards = new List<Card> { converted.Use(decision.cards) };
            }

            return decision;
        }

        public async Task<PlayDecision> Run(int cardCount, int destCount)
        {
            maxCard = minCard = cardCount;
            maxDest = minDest = destCount;
            return await Run();
        }

        private async Task<PlayDecision> WaitDecision()
        {
            var message = await game.eventSystem.PopDecision() as Model.PlayDecision;
            // Delay.StopAll();
            return new PlayDecision(message, this);
        }

        protected async void AutoDecision(CancellationToken cancellationToken)
        {
            PlayDecision decision = null;
            // switch (MCTS.Instance.state)
            // {
            //     case MCTS.State.Disable:
            if (player.isAI)
            {
                await Delay.Run(1000);
                // await new Delay(1f).Run();
                // decision = aiAct ? defaultAI() : new PlayDecision();
                if (aiAct)
                {
                    game.ai.playQuery = this;
                    defaultAI ??= game.ai.TryAction;
                    decision = defaultAI();
                    decision.action = true;
                }
                else decision = new();
            }
            else
            {
                // if (!await new Delay(second).Run()) return;

                try { await Delay.Run(second * 1000, cancellationToken); }
                catch (TaskCanceledException) { return; }

                decision = new PlayDecision();
            }
            //         break;
            //     case MCTS.State.Ready:
            //         if (players[0].isSelf)
            //         {
            //             if (!await new Delay(second).Run()) return;
            //             decision = new Decision();
            //         }
            //         else if (players[0].isAI)
            //         {
            //             await new Delay(1f).Run();
            //             decision = await MCTS.Instance.Run(MCTS.State.WaitTimer);
            //         }
            //         break;
            //     case MCTS.State.Restoring:
            //         if (Decision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitTimer;
            //         return;
            //     case MCTS.State.Simulating:
            //         decision = defaultAI();
            //         break;
            // }

            // SendDecision(decision);
            decision.src ??= player;
            var message = decision.ToMessage();
            // if(message.player==-1) message.player=player.position;
            message.skill ??= skill;
            game.eventSystem.PushDecision(message);
        }
    }

    /// <summary>
    /// 用于暂停主线程，并获得玩家的操作结果
    /// </summary>
    // public class Timer : Singleton<Timer>
    // {
    //     // public List<Player> players { get; protected set; } = new();
    //     public PlayRequest playRequest { get; private set; }
    //     // public PlayDecision temp { get; protected set; } = new();
    //     // public Func<Decision> defaultAI { get; set; } = () => new Decision();

    //     // #region  以下属性用于规定玩家的操作方式，如可指定的目标，可选中的牌等

    //     // private int _maxCard;
    //     // private int _minCard;
    //     // private Func<int> _maxDest = () => 0;
    //     // private Func<int> _minDest = () => 0;
    //     // private Predicate<Card> _isValidCard = x => x.discardable;
    //     // private Predicate<Player> _isValidDest = x => true;

    //     // public int maxCard
    //     // {
    //     //     get => temp.skill is null ? _maxCard : temp.skill.MaxCard;
    //     //     set => _maxCard = value;
    //     // }
    //     // public int minCard
    //     // {
    //     //     get => temp.skill is null ? _minCard : temp.skill.MinCard;
    //     //     set => _minCard = value;
    //     // }
    //     // public Predicate<Card> isValidCard
    //     // {
    //     //     get => temp.skill is null ? _isValidCard : temp.skill.IsValidCard;
    //     //     set => _isValidCard = value;
    //     // }
    //     // public Func<int> maxDest
    //     // {
    //     //     get => temp.skill is null || temp.skill is GameCore.Converted ? _maxDest : () => temp.skill.MaxDest;
    //     //     set => _maxDest = value;
    //     // }
    //     // public Func<int> minDest
    //     // {
    //     //     get => temp.skill is null || temp.skill is GameCore.Converted ? _minDest : () => temp.skill.MinDest;
    //     //     set => _minDest = value;
    //     // }
    //     // public Predicate<Player> isValidDest
    //     // {
    //     //     get => temp.skill is null || temp.skill is GameCore.Converted ? _isValidDest : temp.skill.IsValidDest;
    //     //     set => _isValidDest = value;
    //     // }

    //     // public enum Type
    //     // {
    //     //     Normal,
    //     //     WXKJ,
    //     //     Compete,
    //     //     InPlayPhase
    //     // }

    //     // public Type type { get; set; } = Type.Normal;
    //     // 可取消，即是否显示取消按钮
    //     // public bool refusable { get; set; } = true;
    //     // 转换牌列表，如仁德选择一种基本牌
    //     // public List<Card> multiConvert { get; } = new();

    //     // public Equipment equipSkill { get; set; }
    //     // public string hint { get; set; }
    //     // public int second { get; protected set; }

    //     // #endregion

    //     // public bool isDone{get;private set;}=true;
    //     // int ttt = 0;


    //     // public async Task<Decision> Run(Model.StartPlay startPlay)
    //     // {
    //     //     // 
    //     // }

    //     // public Timer()
    //     // {
    //     //     game.eventSystem.AddEvent<Model.PlayDecision>(OnMessage);
    //     // }

    //     /// <summary>
    //     /// 暂停主线程，等待玩家传入操作结果
    //     /// </summary>
    //     public async Task<PlayDecision> Run(PlayRequest startPlay)
    //     {
    //         // var player = startPlay.player;
    //         // players.Add(startPlay.player);
    //         this.playRequest = startPlay;
    //         // temp.src = startPlay.player;
    //         // second = minCard > 1 ? 10 + minCard : 15;

    //         // var secondValiddests = cards.Select(x =>
    //         // {
    //         //     // int index = validdests.IndexOf(dests);
    //         //     var firsts = validdests[cards.IndexOf(x)];
    //         //     return firsts.Select(first => game.players.Where(p => condition.isValidSecondDest(p, first, x)));
    //         // });
    //         // if (condition.maxCard == 1 && condition.minCard == 1)
    //         // {
    //         //     for (int i = 0; i < cards.Count; i++)
    //         //     {
    //         //         var card = cards[i];
    //         //     }
    //         // }

    //         // if (player.isSelf && !MCTS.Instance.isRunning)
    //         // {
    //         //     await Main.Instance.MoveSeat(player);
    //         // }
    //         // isDone=false;
    //         // StartTimerView?.Invoke();
    //         var message = startPlay.ToMessage();


    //         // foreach (var i in condition.virtualCards)
    //         // {
    //         //     if (condition.isValidVirtualCard(i)) startPlay.vtlCards.Add(i.name);
    //         //     else startPlay.disabledVtlCards.Add(i.name);
    //         // }

    //         message.skills = startPlay.player.skills
    //             .Where(x => (x is Active || x is Converted) && x.IsValid)
    //             .Select(x => x.ToStartPlay().ToMessage())
    //             .ToList();

    //         game.eventSystem.Send(message);
    //         // startPlay.vtlCards = condition.virtualCards.Select(x => x.name).ToList();
    //         // if (startPlay.vtlCards.Count > 0) startPlay.disabledVtlCards
    //         //     = condition.virtualCards
    //         //     .Where(x => condition.isValidVirtualCard(x))
    //         //     .Select(x => x.name)
    //         //     .ToList();

    //         // if (condition is DiffDest diffDest)
    //         // {
    //         //     int length = startPlay.givenCards.Count;
    //         //     var st = startPlay as Model.StartPlay.DiffDest;
    //         //     st.maxDestForCard = new int[length];
    //         //     st.minDestForCard = new int[length];
    //         //     st.givenDestsForCard = new int[length][];
    //         //     for (int i = 0; i < length; i++)
    //         //     {
    //         //         var card = game.cardPile.cards[st.givenCards[i]];
    //         //         st.maxDestForCard[i] = diffDest.maxDestForCard(card);
    //         //         st.minDestForCard[i] = diffDest.minDestForCard(card);
    //         //         st.givenDestsForCard[i] = game.players.Where(x => diffDest.isValidDestForCard(x, card)).Select(x => x.position).ToArray();
    //         //     }
    //         // }
    //         // game.eventSystem.Send(new)

    //         await AutoDecision();

    //         var decision = await WaitResult();
    //         // if (this == MCTS.Instance._timer) Debug.Log(888);
    //         // if (Decision.List.Instance == MCTS.Instance._decisionList) Debug.Log(999);
    //         // Debug.Log(ttt++);

    //         if (!decision.action && !startPlay.refusable)
    //         {
    //             decision.action = true;
    //             if (startPlay.minCard > 0)
    //             {
    //                 // var cards = player.HandCards.Union(player.Equipments.Values).Where(x => isValidCard(x));
    //                 decision.cards = startPlay.player.cards
    //                     .Where(x => startPlay.isValidCard(x))
    //                     .Take(startPlay.minCard)
    //                     .ToList();
    //             }
    //             if (startPlay.maxDest > 0)
    //             {
    //                 decision.dests = game.AlivePlayers
    //                     .Where(x => startPlay.isValidDest(x))
    //                     .Take(startPlay.minDest)
    //                     .ToList();
    //             }
    //         }

    //         // Reset();
    //         game.eventSystem.Send(new Model.FinishPlay
    //         {
    //             player = startPlay.player.position,
    //             type = startPlay.type
    //         });

    //         if (decision.skill is Converted converted)
    //         {
    //             decision.cards = new List<Card> { converted.Use(decision.cards) };
    //         }

    //         return decision;
    //     }

    //     public async Task<PlayDecision> Run(PlayRequest startPlay, int cardCount, int destCount)
    //     {
    //         startPlay.maxCard = startPlay.minCard = cardCount;
    //         startPlay.maxDest = startPlay.minDest = destCount;
    //         return await Run(startPlay);
    //         // this.maxCard = cardCount;
    //         // this.minCard = cardCount;

    //         // if (destCount > 0)
    //         // {
    //         //     this.maxDest = () => destCount;
    //         //     this.minDest = () => destCount;
    //         // }

    //         // return await Run(player);
    //     }

    //     // private TaskCompletionSource<Model.PlayDecision> tcs=new();

    //     // private void OnMessage(Model.PlayDecision message)
    //     // {
    //     //     tcs.SetResult(message);
    //     // }

    //     protected async Task<PlayDecision> WaitResult()
    //     {
    //         // tcs=new TaskCompletionSource<Model.PlayDecision>();
    //         // return new PlayDecision(await tcs.Task,game);
    //         return new PlayDecision(await game.eventSystem.PopDecision(), playRequest);
    //         // if (!Room.Instance.IsSingle)
    //         // {
    //         //     var message = await WebSocket.Instance.PopMessage();
    //         //     var json = JsonUtility.FromJson<PlayDecision.DMessage>(message);

    //         //     PlayDecision.List.Instance.Push(json);
    //         // }

    //         // // var d = await Decision.Pop();
    //         // // Util.Print("hint=" + hint + "\ndecision=\n" + d);
    //         // // return d;
    //         // return await PlayDecision.List.Instance.Pop();
    //     }

    //     // public void SendDecision(PlayDecision decision)
    //     // {
    //     //     // Util.Print(Decision.List.Instance);
    //     //     if (decision is null)
    //     //     {
    //     //         decision = temp;
    //     //         // temp = new Decision();
    //     //     }
    //     //     // Delay.StopAll();

    //     //     // if (!decision.action)
    //     //     // {
    //     //     //     decision.cards.Clear();
    //     //     //     decision.dests.Clear();
    //     //     //     decision.skill = null;
    //     //     //     decision.other=null;
    //     //     //     // decision.converted = null;
    //     //     // }

    //     //     if (temp.cards.FirstOrDefault() is Card card && card.isConvert)
    //     //     {
    //     //         decision.virtualCard = card.name;
    //     //         decision.cards.Clear();
    //     //     }

    //     //     if (Room.Instance.IsSingle) PlayDecision.List.Instance.Push(decision);
    //     //     else
    //     //     {
    //     //         var json = decision.ToMessage();
    //     //         if (decision.src != null) json.src = decision.src.position;

    //     //         WebSocket.Instance.SendMessage(json);
    //     //     }
    //     //     // Util.Print(Decision.List.Instance);
    //     //     // Util.Print("4");
    //     // }

    //     // public PlayDecision SaveTemp()
    //     // {
    //     //     var t = temp;
    //     //     temp = new Decision();
    //     //     return t;
    //     // }

    //     // private void Reset()
    //     // {
    //     //     temp = new Decision();
    //     //     // isDone=true;
    //     //     StopTimerView?.Invoke();

    //     //     players.Clear();
    //     //     hint = "";
    //     //     maxCard = 0;
    //     //     minCard = 0;
    //     //     maxDest = () => 0;
    //     //     minDest = () => 0;
    //     //     isValidCard = card => card.discardable;
    //     //     isValidDest = dest => true;
    //     //     equipSkill = null;
    //     //     type = Type.Normal;
    //     //     refusable = true;
    //     //     multiConvert.Clear();
    //     //     defaultAI = () => new Decision();
    //     // }

    //     protected async Task AutoDecision()
    //     {
    //         PlayDecision decision = null;
    //         // switch (MCTS.Instance.state)
    //         // {
    //         //     case MCTS.State.Disable:
    //         if (playRequest.player.isAI)
    //         {
    //             await new Delay(1f).Run();
    //             AI.Instance.playRequest = playRequest;
    //             decision = playRequest.aiAct ? playRequest.defaultAI() : new PlayDecision();
    //         }
    //         else
    //         {
    //             if (!await new Delay(playRequest.second).Run()) return;
    //             decision = new PlayDecision();
    //         }
    //         //         break;
    //         //     case MCTS.State.Ready:
    //         //         if (players[0].isSelf)
    //         //         {
    //         //             if (!await new Delay(second).Run()) return;
    //         //             decision = new Decision();
    //         //         }
    //         //         else if (players[0].isAI)
    //         //         {
    //         //             await new Delay(1f).Run();
    //         //             decision = await MCTS.Instance.Run(MCTS.State.WaitTimer);
    //         //         }
    //         //         break;
    //         //     case MCTS.State.Restoring:
    //         //         if (Decision.List.Instance.IsEmpty) MCTS.Instance.state = MCTS.State.WaitTimer;
    //         //         return;
    //         //     case MCTS.State.Simulating:
    //         //         decision = defaultAI();
    //         //         break;
    //         // }

    //         // SendDecision(decision);
    //         game.eventSystem.PushDecision(decision.ToMessage());
    //     }

    //     // public async Task<PlayDecision> RunWxkj(Card scheme, Model.Team team)
    //     // {
    //     //     players.AddRange(team.GetAllPlayers());
    //     //     hint = scheme + "即将对" + scheme.dest + "生效，是否使用无懈可击？";
    //     //     maxCard = 1;
    //     //     minCard = 1;
    //     //     isValidCard = x => x is 无懈可击;
    //     //     type = Type.WXKJ;
    //     //     defaultAI = () =>
    //     //     {
    //     //         foreach (var i in players)
    //     //         {
    //     //             var card = i.FindCard<无懈可击>();
    //     //             if (card is null || scheme.src.team == team) continue;

    //     //             return new Decision { src = i, action = true, cards = new List<Card> { card } };
    //     //         }
    //     //         return new();
    //     //     };

    //     //     StartTimerView?.Invoke();
    //     //     await AutoDecision();
    //     //     var decision = await WaitResult();
    //     //     Reset();
    //     //     return decision;
    //     // }

    //     // public async Task<Decision> RunCompete(Player player0,Player player1)
    //     // {
    //     //     players.Add(player0);
    //     //     players.Add(player1);
    //     //     hint = "请选择一张手牌拼点";
    //     //     maxCard = 1;
    //     //     minCard = 1;
    //     //     isValidCard = x => x.IsHandCard;
    //     //     type = Type.Compete;
    //     //     DefaultAI = () =>

    //     //     StartTimerView?.Invoke();
    //     //     await AutoDecision();
    //     //     var decision = await WaitResult();
    //     //     Reset();
    //     //     return decision;
    //     // }


    //     // public Action StartTimerView;
    //     // public Action StopTimerView;

    //     // protected static Timer currentInstance;
    //     // public static new Timer Instance => currentInstance is null ? Singleton<Timer>.Instance : currentInstance;
    // }
}