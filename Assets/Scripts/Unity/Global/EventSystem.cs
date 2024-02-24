using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Model;

public class EventSystem : GlobalSingleton<EventSystem>
{
    public EventSystem()
    {
        Run();
    }

    private async void Run()
    {
        while (true)
        {
            await Lock();
            await semaphoreSlim.WaitAsync();
            Invoke(messages.Dequeue());
            UnLock();
            await Task.Yield();
        }
    }

    private Queue<Message> messages = new();
    private SemaphoreSlim semaphoreSlim = new(0);

    private Dictionary<Type, Delegate> events = new();
    private Dictionary<Type, Delegate> priorEvents = new();

    private SemaphoreSlim _lock = new(1);
    public Task Lock() => _lock.WaitAsync();
    public void UnLock() => _lock.Release();

    public void OnMessage(Message json)
    {
        messages.Enqueue(json);
        semaphoreSlim.Release();
    }

    private void Invoke(Message message)
    {
        var type = message.GetType();

        for (var i = type; i != null; i = i.BaseType)
        {
            if (priorEvents.ContainsKey(i)) priorEvents[i]?.DynamicInvoke(message);
        }

        for (var i = type; i != null; i = i.BaseType)
        {
            if (events.ContainsKey(i)) events[i]?.DynamicInvoke(message);
        }
    }

    public void AddEvent<T>(Action<T> action) where T : Message
    {
        Type type = typeof(T);
        if (!events.ContainsKey(type)) events.Add(type, null);
        events[type] = Delegate.Combine(events[type], action);
    }

    public void RemoveEvent<T>(Action<T> action) where T : Message
    {
        Type type = typeof(T);
        events[type] = Delegate.Remove(events[type], action);
    }

    public void AddPriorEvent<T>(Action<T> action) where T : Message
    {
        Type type = typeof(T);
        if (!priorEvents.ContainsKey(type)) priorEvents.Add(type, null);
        priorEvents[type] = Delegate.Combine(priorEvents[type], action);
    }

    public void RemovePriorEvent<T>(Action<T> action) where T : Message
    {
        Type type = typeof(T);
        priorEvents[type] = Delegate.Remove(priorEvents[type], action);
    }

    public void SendToServer(Message message)
    {
        if (Global.Instance.IsStandalone) Game.Instance.game.eventSystem.PushMessage(message);
        else Connection.Instance.SendGameMessage(message);
    }
}
