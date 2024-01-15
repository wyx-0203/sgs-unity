using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;

public class EventSystem : GlobalSingletonMono<EventSystem>
{
    private async void Start()
    {
        model = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Models");
        // Type type = model.GetType("Models.EventCenter");
        // PropertyInfo property = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        // GameCore.EventSystem.Instance.OnMessage1 += OnMessage;
        GameCore.EventSystem.Instance.OnMessage1 += x =>
        {
            messages.Enqueue(x);
            semaphoreSlim.Release();
        };

        while (true)
        {
            await semaphoreSlim.WaitAsync();
            Invoke(messages.Dequeue());
            await Task.Yield();
        }
    }

    private Assembly model;
    private Queue<string> messages = new();
    private SemaphoreSlim semaphoreSlim = new(0);

    private Dictionary<Type, Delegate> events = new();
    private Dictionary<Type, Delegate> priorEvents = new();

    private void Invoke(string json)
    {
        UnityEngine.Debug.Log(json);
        var type = model.GetType(JsonConvert.DeserializeObject<Message>(json)._type);
        var message = JsonConvert.DeserializeObject(json, type);

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

    public void Send(Message message)
    {
        if (message is Decision decision) GameCore.EventSystem.Instance.PushDecision(decision);
        else Invoke(JsonConvert.SerializeObject(message));
    }
}
