using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class EventData { }

    public class EventManager
    {
        public static EventManager Default = new EventManager();

        private readonly Dictionary<string, List<Action<EventData>>> _handlers = new Dictionary<string, List<Action<EventData>>>();

        public void AddHandler(string name, Action<EventData> handler)
        {
            if (!_handlers.TryGetValue(name, out List<Action<EventData>> eventHandlers))
                eventHandlers = new List<Action<EventData>>();

            eventHandlers.Add(handler);

            _handlers.Add(name, eventHandlers);
        }

        public void RemoveHandler(string name, Action<EventData> handler)
        {
            if (!_handlers.TryGetValue(name, out List<Action<EventData>> eventHandlers))
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

        public void Dispatch(string name, EventData data = null)
        {
            if (!_handlers.TryGetValue(name, out List<Action<EventData>> eventHandlers))
                return;

            for(int i = 0; i < eventHandlers.Count; i++)
            {
                eventHandlers[i](data);
            }
        }
    }
}
