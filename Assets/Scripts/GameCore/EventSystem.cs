using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Model;

namespace GameCore
{
    public class EventSystem
    {
        public void SendToClient(Message message)
        {
            onSendToClient?.Invoke(message);

            if (surrender != null)
            {
                var team = surrender.team;
                surrender = null;
                throw new GameOverException(team);
            }
        }

        private readonly Action<Message> onSendToClient;

        public EventSystem(Action<Message> onSendToClient)
        {
            this.onSendToClient = onSendToClient;
        }

        private List<Decision> decisions = new();
        private int front = 0;
        private SemaphoreSlim semaphoreSlim = new(0);
        private Surrender surrender = null;

        public void PushMessage(Message message)
        {
            switch (message)
            {
                case Decision decision:
                    PushDecision(decision);
                    break;
                case Surrender surrender:
                    this.surrender = surrender;
                    PushDecision(null);
                    break;
                default:
                    SendToClient(message);
                    break;
            }
        }

        internal void PushDecision(Decision decision)
        {
            decisions.Add(decision);
            semaphoreSlim.Release();
        }

        internal async Task<Decision> PopDecision()
        {
            await semaphoreSlim.WaitAsync();
            return decisions[front++];
        }
    }
}
