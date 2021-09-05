using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class EventData
    {

    }

    public class EventManager
    {
        public static EventManager main = new EventManager();

        private readonly Dictionary<string, List<Func<EventData, bool>>> _handlers = new Dictionary<string, List<Func<EventData, bool>>>();

        public void AddHandler<T>(Func<T, bool> handler) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName ?? string.Empty, out List<Func<EventData, bool>> eventHandlers))
                eventHandlers = new List<Func<EventData, bool>>();

            eventHandlers.Add(e => handler.Invoke((T)e));

            _handlers[typeof(T).FullName ?? string.Empty] = eventHandlers;
        }

        public void RemoveHandler<T>(Func<T, bool> handler) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName ?? string.Empty, out List<Func<EventData, bool>> eventHandlers))
                return;

            for(int i = 0; i < eventHandlers.Count; i++)
            {
                if (eventHandlers[i] != handler) 
                    continue;

                eventHandlers.RemoveAt(i);
                return;
            }
        }

        public void Dispatch<T>(T data = null) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName ?? string.Empty, out List<Func<EventData, bool>> eventHandlers))
                return;

            foreach (var handler in eventHandlers)
            {
                if(handler(data))
                {
                    break;
                }
            }
        }
    }
}
