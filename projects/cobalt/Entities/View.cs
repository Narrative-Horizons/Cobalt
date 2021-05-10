using Cobalt.Entities.Components;
using System;
using System.Collections.Generic;

namespace Cobalt.Entities
{
    /// <summary>
    /// Gets a view of a single component type.
    /// </summary>
    /// <typeparam name="Component">Type of the component</typeparam>
    public class ComponentView<Component> where Component : BaseComponent, new()
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

        public delegate void EntityOperation(Entity ent, Registry reg);

        public EntityView(Registry reg)
        {
            _reg = reg;
        }

        public void ForEach(EntityOperation op)
        {
            foreach (Entity ent in _reg.GetEntities())
            {
                op.Invoke(ent, _reg);
            }
        }

        // TODO: Filtering
    }
}
