using System.Collections;
using System.Threading.Tasks;
using Model;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
    public static async Task WaitFrame(this MonoBehaviour _, int frames)
    {
        // var tcs=new TaskCompletionSource<object>();
        int t = Time.frameCount + frames;
        while (t != Time.frameCount) await Task.Yield();
    }

    public static IEnumerator FadeIn(this Graphic graphic, float secends)
    {
        graphic.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / secends;
            graphic.color = new(graphic.color.r, graphic.color.g, graphic.color.b, Mathf.Lerp(0, 1, t));
            yield return null;
        }
    }

    public static IEnumerator FadeOut(this Graphic graphic, float secends)
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / secends;
            graphic.color = new(graphic.color.r, graphic.color.g, graphic.color.b, Mathf.Lerp(0, 1, t));
            yield return null;
        }
        graphic.gameObject.SetActive(false);
    }


    public static IEnumerator FadeIn(this CanvasGroup canvasGroup, float secends)
    {
        canvasGroup.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / secends;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
    }

    public static IEnumerator FadeOut(this CanvasGroup canvasGroup, float secends)
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / secends;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        canvasGroup.gameObject.SetActive(false);
    }
}

public class Util : GlobalSingletonMono<Util>
{

    public static async Task WaitFrame(int count = 1)
    {
        int t = Time.frameCount + count;
        while (t != Time.frameCount) await Task.Yield();
    }

    public static void Print(object str)
    {
        Debug.Log(str);
    }

    public static async Task<UserInfoResponse> GetUserInfo(int userId)
    {
        var msg = await WebRequest.Get($"{Url.DOMAIN_NAME}getUserInfo?id={userId}");
        return JsonUtility.FromJson<Model.UserInfoResponse>(msg);
    }
}

// public class Delay
// {
//     private static List<Delay> list = new List<Delay>();
//     private IEnumerator coroutine;
//     private float second;
//     private bool isDone;
//     private bool isValid;

//     public Delay(float second)
//     {
//         this.second = second;
//     }

//     /// <summary>
//     /// 延迟指定秒数，若被打断则返回false
//     /// </summary>
//     public async Task<bool> Run()
//     {
//         // if (GameCore.MCTS.Instance.isRunning) return true;
//         list.Add(this);
//         coroutine = RunCoroutine(second);
//         Util.Instance.StartCoroutine(coroutine);
//         while (!isDone) await Task.Yield();
//         return isValid;
//     }

//     public void Stop()
//     {
//         Util.Instance.StopCoroutine(coroutine);
//         isValid = false;
//         isDone = true;
//     }

//     public static void StopAll()
//     {
//         // if (GameCore.MCTS.Instance.isRunning) return;
//         Util.Instance.StopAllCoroutines();
//         foreach (var i in list)
//         {
//             i.isValid = false;
//             i.isDone = true;
//         }
//     }

//     private IEnumerator RunCoroutine(float second)
//     {
//         yield return new WaitForSeconds(second);
//         isValid = true;
//         isDone = true;
//     }
// }

