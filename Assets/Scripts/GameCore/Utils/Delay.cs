using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class Delay
{

    /// <summary>
    /// 延迟指定秒数，若被打断则返回false
    /// </summary>
    public static Task Run(int millisecondsDelay, CancellationToken cancellationToken)
    {
        return Task.Delay(millisecondsDelay, cancellationToken);
    }

    public static Task Run(int millisecondsDelay) => Task.Delay(millisecondsDelay);
}

public static class ListExtensions
{
    public static List<T> Shuffle<T>(this IEnumerable<T> enumerable, int count)
    {
        if (enumerable is not List<T> list) list = enumerable.ToList();
        // var list = enumerable.ToList();
        var random = new Random();

        // 随机取一个元素与第i个元素交换
        for (int i = 0; i < count; i++)
        {
            int t = random.Next(i, list.Count);
            var item = list[i];
            list[i] = list[t];
            list[t] = item;
        }
        return list.GetRange(0, count);


        // for (int i = list.Count - 1; i > 0; i--)
        // {
        //     int randomIndex = random.Next(0, i + 1);

        //     // 交换当前元素与随机索引处的元素
        //     T temp = list[i];
        //     list[i] = list[randomIndex];
        //     list[randomIndex] = temp;
        // }

        // return list;
    }

    public static List<T> Shuffle<T>(this IEnumerable<T> enumerable) => Shuffle(enumerable, enumerable.Count());

    public static T GetRandomOne<T>(this List<T> list) => list[new Random().Next(0, list.Count)];
}

