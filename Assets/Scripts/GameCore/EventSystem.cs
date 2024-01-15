using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
// using UnityEngine;
using Newtonsoft.Json;

namespace GameCore
{
    public class EventSystem : Singleton<EventSystem>
    {
        // public List<Model.Message> messages { get; } = new();

        public void Send(Model.Message message)
        {
            // messages.Add(message);
            OnMessage1?.Invoke(JsonConvert.SerializeObject(message));
        }

        public Action<string> OnMessage1;

        private Dictionary<Type, Delegate> events = new();

        public EventSystem()
        {
            model = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Models");
        }
        public Assembly model;
        private List<Model.Decision> decisions = new();
        private int front = 0;
        private SemaphoreSlim semaphoreSlim = new(0);

        private void OnMessage(string json)
        {
            Type type = model.GetType(JsonConvert.DeserializeObject<Model.Message>(json)._type);
            var message = JsonConvert.DeserializeObject(json, type) as Model.Message;
            if (message is Model.Decision decision) PushDecision(decision);
            else events[type]?.DynamicInvoke(message);
        }

        public void PushDecision(Model.Decision decision)
        {
            decisions.Add(decision);
            semaphoreSlim.Release();
        }

        public async Task<Model.Decision> PopDecision()
        {
            await semaphoreSlim.WaitAsync();
            return decisions[front++];
        }

        public void AddEvent<T>(Action<T> action) where T : Model.Message
        {
            Type type = typeof(T);
            if (!events.ContainsKey(type)) events.Add(type, null);
            events[type] = Delegate.Combine(events[type], action);
        }

        public void RemoveEvent<T>(Action<T> action) where T : Model.Message
        {
            Type type = typeof(T);
            events[type] = Delegate.Remove(events[type], action);
        }
    }
}
