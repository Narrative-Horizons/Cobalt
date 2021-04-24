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

        public ComponentView(ref MemoryPool<Component> pool)
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
}
