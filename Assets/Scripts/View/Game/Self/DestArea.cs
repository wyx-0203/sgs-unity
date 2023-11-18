using System.Collections.Generic;
using System.Linq;

public class DestArea : SingletonMono<DestArea>
{
    private List<Dest> players;
    private List<GameCore.Player> SelectedPlayer => GameCore.Timer.Instance.temp.dests;
    private GameCore.Timer timer => GameCore.Timer.Instance;

    private int maxCount;
    private int minCount;
    public bool IsValid { get; private set; } = false;

    private void Start()
    {
        GameCore.Timer.Instance.StopTimerView += Reset;
        players = GameMain.Instance.players.Select(x => x.GetComponent<Dest>()).ToList();
    }

    private void OnDestroy()
    {
        GameCore.Timer.Instance.StopTimerView -= Reset;
    }

    /// <summary>
    /// 初始化目标区
    /// </summary>
    public void OnStartPlay()
    {
        if (!CardArea.Instance.IsValid) return;

        // 设置可选目标数量

        maxCount = timer.maxDest();
        minCount = timer.minDest();

        // 对不能选择的角色设置阴影
        foreach (var i in players) i.AddShadow();

        Update_();

        // 自动选择

        var validDests = players.Where(x => x.toggle.interactable);
        if (validDests.Count() == 1 && minCount == 1)
        {
            foreach (var i in validDests) i.toggle.isOn = true;
        }
    }

    /// <summary>
    /// 重置目标区
    /// </summary>
    public void Reset()
    {
        if (!timer.players.Contains(GameMain.Instance.self.model)) return;

        // 重置目标按键状态
        foreach (var i in players) i.Reset();

        IsValid = false;
    }

    public void Update_()
    {
        // 若已选中角色的数量超出范围，取消第一个选中的角色
        while (SelectedPlayer.Count > maxCount) players.Find(x => x.model == SelectedPlayer[0]).Unselect();

        IsValid = SelectedPlayer.Count >= minCount;
        if (maxCount == 0) return;

        // 每指定一个角色，都要更新不能指定的角色，例如明策指定一个目标后，第二个目标需在第一个的攻击范围内
        foreach (var i in players) i.toggle.interactable = GameCore.Timer.Instance.isValidDest(i.model);
    }
}