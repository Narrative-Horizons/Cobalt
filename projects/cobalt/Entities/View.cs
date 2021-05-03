using System;
using System.Collections.Generic;

namespace Cobalt.Entities
{
    /// <summary>
    /// Gets a view of a single component type.
    /// </summary>
    /// <typeparam name="Component">Type of the component</typeparam>
    public class ComponentView<Component> where Component : unmanaged
    {
        private readonly MemoryPool<Component> _pool;

        public uint Count => _pool.Count;

        public delegate void ComponentOperation(ref Component c);

        internal ComponentView(ref MemoryPool<Component> pool)
        {
            _pool = pool;
        }

        public void ForEach(ComponentOperation op)
        {
            foreach (ref Component comp in _pool.GetPayloadEnumerable())
            {
                op.Invoke(ref comp);
            }
        }

        // TODO: Filtering
    }

    public class EntityView
    {
        private readonly Registry _reg;

        public delegate void EntityOperation(ref Entity ent, Registry reg);

        public EntityView(Registry reg)
        {
            _reg = reg;
        }

        public void ForEach(EntityOperation op)
        {
            foreach (ref Entity ent in _reg.GetEntities())
            {
                op.Invoke(ref ent, _reg);
            }
        }

        // TODO: Filtering
    }
}
