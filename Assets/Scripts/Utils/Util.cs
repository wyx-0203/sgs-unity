// using GameCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Util : GlobalSingletonMono<Util>
{
    public static async Task WaitFrame(int count = 1)
    {
        int t = Time.frameCount + count;
        while (t != Time.frameCount) await Task.Yield();
    }


    /// <summary>
    /// 从当前回合玩家开始，循环执行操作
    /// </summary>
    // public static async Task Loop(Func<Model.Player, Task> func, Func<bool> condition = null)
    // {
    //     var currentPlayer = Model.TurnSystem.Instance.CurrentPlayer;
    //     if (!currentPlayer.IsAlive) currentPlayer = currentPlayer.next;
    //     bool t = true;

    //     for (var i = currentPlayer; i != currentPlayer || t; i = i.next)
    //     {
    //         t = false;

    //         // if (condition?.Invoke() == false) return;
    //         if (condition != null && !condition()) return;
    //         await func(i);
    //     }
    // }

    // public static IEnumerable<Model.Player> PlayersOrderbyPosition()
    // {
    //     var currentPlayer = Model.TurnSystem.Instance.CurrentPlayer;
    //     var length = Model.SgsMain.Instance.players.Length;
    //     return Model.SgsMain.Instance.AlivePlayers.OrderBy(x => (x.position - currentPlayer.position + length) % length);
    // }

    public static void Print(object str)
    {
        // if (!GameCore.MCTS.Instance.isRunning)
        Debug.Log(str);
    }
    // public static string CardsToString(List<Model.Card> cards)
    // {
    //     // 
    // }
    // public static string GetGameInfo()
    // {
    //     string str = "GameInfo:";
    //     str += "\nMCTS.state=" + GameCore.MCTS.Instance.state;
    //     str += "\nplayers:\n" + string.Join("\n", GameCore.SgsMain.Instance.AlivePlayers.Select(x => x.DebugInfo()));
    //     str += "\ndecisions:\n" + GameCore.Decision.List.Instance;
    //     return str;
    // }

}

public class Delay
{
    private static List<Delay> list = new List<Delay>();
    private IEnumerator coroutine;
    private float second;
    private bool isDone;
    private bool isValid;

    public Delay(float second)
    {
        this.second = second;
    }

    /// <summary>
    /// 延迟指定秒数，若被打断则返回false
    /// </summary>
    public async Task<bool> Run()
    {
        // if (GameCore.MCTS.Instance.isRunning) return true;
        list.Add(this);
        coroutine = RunCoroutine(second);
        Util.Instance.StartCoroutine(coroutine);
        while (!isDone) await Task.Yield();
        return isValid;
    }

    public void Stop()
    {
        Util.Instance.StopCoroutine(coroutine);
        isValid = false;
        isDone = true;
    }

    public static void StopAll()
    {
        // if (GameCore.MCTS.Instance.isRunning) return;
        Util.Instance.StopAllCoroutines();
        foreach (var i in list)
        {
            i.isValid = false;
            i.isDone = true;
        }
    }

    private IEnumerator RunCoroutine(float second)
    {
        yield return new WaitForSeconds(second);
        isValid = true;
        isDone = true;
    }
}

