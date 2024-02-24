using Model;
using System.Collections.Generic;
using System.Linq;

public class DestArea : SingletonMono<DestArea>
{
    private List<Dest> players;
    // private List<GameCore.Player> SelectedPlayer => GameCore.Timer.Instance.temp.dests;
    private PlayDecision decision => PlayArea.Instance.decision;
    // private List<int> SelectedCards => OperationArea.Instance.decision.cards;
    private SinglePlayQuery playQuery => PlayArea.Instance.current;
    private SinglePlayQuery.DestInfo destInfo;

    // private int maxCount;
    // private int minCount;
    public bool IsValid { get; private set; } = false;

    private void Start()
    {
        // GameCore.Timer.Instance.StopTimerView += Reset;
        EventSystem.Instance.AddEvent<FinishPlay>(OnReset);
        players = Game.Instance.players.Select(x => x.GetComponent<Dest>()).ToList();
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<FinishPlay>(OnReset);
        // GameCore.Timer.Instance.StopTimerView -= Reset;
    }

    // private int[][] secondDests;

    /// <summary>
    /// 初始化目标区
    /// </summary>
    public void OnStartPlay()
    {
        if (!VirtualCardArea.Instance.IsValid) return;
        // UnityEngine.Debug.Log("11");

        var card = decision.virtualCard != 0 ? decision.virtualCard : decision.cards.FirstOrDefault();
        destInfo = playQuery.destInfos.Find(x => x.cards.Contains(card)) ?? playQuery.destInfos.First();
        // if(destInfo==null)

        // if (this.destInfo is StartPlay.DiffDest diffDest)
        // {
        //     int index = diffDest.disabledVtlCards.Count == 0
        //         ? diffDest.givenCards.IndexOf(decision.cards[0])
        //         : diffDest.disabledVtlCards.IndexOf(decision.other);

        //     this.destInfo.maxDest = diffDest.maxDestForCard[index];
        //     this.destInfo.minDest = diffDest.minDestForCard[index];
        //     this.destInfo.givenDests = diffDest.givenDestsForCard[index];
        //     // startPlay.secondDests = diffDest.secondDests.Length == 0 ? new int[0][] : diffDest.secondDestsForCard[index];
        // }
        if (playQuery.type == SinglePlayQuery.Type.SanYao)
        {
            destInfo.maxDest = destInfo.minDest = decision.cards.Count;
        }

        // 设置可选目标数量
        // maxCount = startPlay.maxDest;
        // minCount = startPlay.minDest;

        // Update_();

        IsValid = destInfo.minDest == 0;
        if (destInfo.maxDest == 0) return;

        foreach (var i in players) i.SetInteractable(destInfo.dests.Contains(i.model.index));
        // {
        //     i.toggle.interactable = ;
        //     i.SetShadow();

        // }

        // 对不能选择的角色设置阴影
        // foreach (var i in players.Where(x => !x.toggle.interactable)) i.SetShadow();

        // 自动选择
        // var validDests = players.Where(x => x.toggle.interactable);
        if (destInfo.dests.Count == 1 && destInfo.minDest == 1)
        {
            players.Find(x => x.model.index == destInfo.dests[0]).toggle.isOn = true;
        }
        // {
        //     foreach (var i in validDests) i.toggle.isOn = true;
        // }
    }

    /// <summary>
    /// 重置目标区
    /// </summary>
    public void Reset()
    {
        // if (!startPlay.players.Contains(GameMain.Instance.self.model)) return;
        IsValid = false;

        // 重置目标按键状态
        foreach (var i in players) i.Reset();

    }

    private void OnReset(FinishPlay finishPlay)
    {
        inReset = true;
        if (Player.IsSelf(finishPlay.player)) Reset();
        inReset = false;
    }

    private bool inReset;

    public void OnClickDest(int player, bool value)
    {
        if (value) decision.dests.Add(player);
        else decision.dests.Remove(player);
        if (inReset) return;

        // 若已选中角色的数量超出范围，取消第一个选中的角色
        while (decision.dests.Count > destInfo.maxDest)
        {
            players.Find(x => x.model.index == decision.dests[0]).toggle.isOn = false;
        }

        IsValid = decision.dests.Count >= destInfo.minDest;
        // if (startPlay.maxDest == 0) return;

        // 每指定一个角色，都要更新不能指定的角色，例如明策指定一个目标后，第二个目标需在第一个的攻击范围内
        // foreach (var i in players) i.toggle.interactable = GameCore.Timer.Instance.isValidDest(i.model);

        // 第二个目标与第一个目标的范围不一样，例如借刀杀人、明策等
        if (destInfo.maxDest == 2 && destInfo.minDest == 2 && destInfo.secondDests.Count > 0)
        {
            // foreach (var i in players)
            // {
            //     if (decision.dests.Count == 0) i.SetInteractable(destInfo.dests.Contains(i.model.index));
            //     else if (decision.dests.Count == 1)
            //     {
            //         int index = destInfo.dests.IndexOf(decision.dests[0]);
            //         i.SetInteractable(destInfo.secondDests[index].Contains(i.model.index) || i.toggle.isOn);
            //     }
            //     else i.SetInteractable(i.toggle.isOn);
            // }
            switch (decision.dests.Count)
            {
                case 0:
                    foreach (var i in players) i.SetInteractable(destInfo.dests.Contains(i.model.index));
                    break;
                case 1:
                    int index = destInfo.dests.IndexOf(decision.dests[0]);
                    if (index == -1)
                    {
                        players.Find(x => x.model.index == decision.dests[0]).toggle.isOn = false;
                        return;
                    }
                    foreach (var i in players)
                    {
                        i.SetInteractable(destInfo.secondDests[index].Contains(i.model.index) || i.toggle.isOn);
                    }
                    break;
                case 2:
                    foreach (var i in players) i.SetInteractable(i.toggle.isOn);
                    break;
            }
        }

        PlayArea.Instance.UpdateButtonArea();
    }
}