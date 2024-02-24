using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : SingletonMono<LogManager>
{
    private readonly Stack<Log> pool = new();
    public Log logPrefab;


    private void Start()
    {
        EventSystem.Instance.AddEvent<Message>(OnMessage);
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<Message>(OnMessage);
    }

    private void OnMessage(Message message)
    {
        if (string.IsNullOrEmpty(message.text)) return;
        message.text = message.text.Replace("♥️", "<color=red>♥️</color>");
        message.text = message.text.Replace("♦️", "<color=red>♦️</color>");
        message.text = message.text.Replace("♠️", "<color=black>♠️</color>");
        message.text = message.text.Replace("♣️", "<color=black>♣️</color>");
        Debug.Log(message.text);
        if (message is UseSkill || message is UseCard) StartCoroutine(OnMessageCor(message.text));
    }

    private IEnumerator OnMessageCor(string text)
    {
        var log = pool.Count > 0 ? pool.Pop() : Instantiate(logPrefab, transform);
        log.text.text = text;
        yield return log.canvasGroup.FadeIn(0.1f);
        yield return new WaitForSeconds(2f);
        yield return log.canvasGroup.FadeOut(0.3f);
        pool.Push(log);
    }
}
