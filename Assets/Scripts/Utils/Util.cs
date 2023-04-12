using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class Util : GlobalSingletonMono<Util>
{
    public async Task WaitFrame(int count = 1)
    {
        int t = Time.frameCount + count;
        while (t != Time.frameCount) await Task.Yield();
    }

    /// <summary>
    /// 队友位置
    /// </summary>
    public int TeammatePos(int position) => 3 - position;

    /// <summary>
    /// 从当前回合玩家开始，循环执行操作
    /// </summary>
    public async Task Loop(Func<Model.Player, Task> func)
    {
        var currentPlayer = Model.TurnSystem.Instance.CurrentPlayer;
        if (!currentPlayer.IsAlive) currentPlayer = currentPlayer.next;
        bool t = true;

        for (var i = currentPlayer; i != currentPlayer || t; i = i.next)
        {
            t = false;
            await func(i);
        }
    }
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