using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class EventData
    {

    }

    public class EventManager
    {
        public static EventManager Default = new EventManager();

        private readonly Dictionary<string, List<Func<EventData, bool>>> _handlers = new Dictionary<string, List<Func<EventData, bool>>>();

        public void AddHandler<T>(Func<T, bool> handler) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName, out List<Func<EventData, bool>> eventHandlers))
                eventHandlers = new List<Func<EventData, bool>>();

            eventHandlers.Add((EventData e) =>
            {
                return handler.Invoke((T)e);
            });

            _handlers.Add(typeof(T).FullName, eventHandlers);
        }

        public void RemoveHandler<T>(Func<T, bool> handler) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName, out List<Func<EventData, bool>> eventHandlers))
                return;

            for(int i = 0; i < eventHandlers.Count; i++)
            {
                if(eventHandlers[i] == handler)
                {
                    eventHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        public void Dispatch<T>(T data = null) where T : EventData
        {
            if (!_handlers.TryGetValue(typeof(T).FullName, out List<Func<EventData, bool>> eventHandlers))
                return;

            for(int i = 0; i < eventHandlers.Count; i++)
            {
                if(eventHandlers[i](data))
                {
                    break;
                }
            }
        }
    }
}
