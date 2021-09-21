using Cobalt.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Cobalt.Entities
{
    /// <summary>
    /// Gets a view of a single component type.
    /// </summary>
    /// <typeparam name="Component">Type of the component</typeparam>
    public class ComponentView<Component> where Component : BaseComponent
    {
        private readonly MemoryPool<Component> _pool;

        public uint Count => _pool.Count;

        public delegate void ComponentOperation(Component c);

        internal ComponentView(MemoryPool<Component> pool)
        {
            _pool = pool;
        }

        public void ForEach(ComponentOperation op)
        {
            foreach (Component comp in _pool.GetPayloadEnumerable())
            {
                op.Invoke(comp);
            }
        }

        // TODO: Filtering
    }

    public class EntityView
    {
        private readonly Registry _reg;
        private List<Type> _types = new List<Type>();

        public delegate void EntityOperation(Entity ent, Registry reg);


        public EntityView(Registry reg)
        {
            _reg = reg;
        }

        public void ForEach(EntityOperation op)
        {
            if (_types.Count == 0)
            {
                foreach (Entity ent in _reg.GetEntities())
                {
                    op.Invoke(ent, _reg);
                }
            }
            else
            {
                foreach (var ent in _reg.GetEntities().Where(ent => _types.Count == (from t in _types where _reg.Has(ent, t) select ent).Count()))
                {
                    op.Invoke(ent, _reg);
                }
            }
        }

        public EntityView Requires<Component>()
        {
            _types.Add(typeof(Component));
            return this;
        }
    }
}
