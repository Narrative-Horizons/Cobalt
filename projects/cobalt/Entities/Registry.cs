using System;
using System.Collections.Generic;

namespace Cobalt.Entities
{
    public class Registry
    {
        private Dictionary<Guid, IMemoryPool> _pools = new Dictionary<Guid, IMemoryPool>();
        private Vector<Entity> _entities = new Vector<Entity>();
        private Entity _next_available = Entity.Invalid;

        public uint Capacity => _entities.Capacity;
        public uint Count => _entities.Count;

        public uint Active()
        {
            uint inactive = 0;

            for (Entity current = _next_available; !current.IsInvalid; ++inactive)
            {
                uint idx = current.Identifier;
                current = _entities[idx];
            }

            return _entities.Count - inactive;
        }

        public Entity Create()
        {
            if (_next_available == Entity.Invalid)
            {
                return CreateNewIdentifier();
            }
            return RecycleIdentifier();
        }

        public void Assign<Component>(Entity ent, ref Component value) where Component : unmanaged
        {
            MemoryPool<Component> pool = GetPool<Component>();
            pool.Assign(ent, ref value);
        }

        public void AssignOrReplace<Component>(Entity ent, ref Component value) where Component : unmanaged
        {
            if (Has<Component>(ent))
            {
                Replace(ent, ref value);
            }
            else
            {
                Assign(ent, ref value);
            }
        }

        public ref Component Get<Component>(Entity ent) where Component : unmanaged
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return ref pool.Get(ent);
        }

        public bool Has<Component>(Entity ent) where Component : unmanaged
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return pool.Contains(ent);
        }

        public void Release(Entity ent)
        {
            // Release components
            foreach (var pool in _pools.Values)
            {
                if (pool.Contains(ent))
                {
                    pool.Remove(ent);
                }
            }

            // Recycle entity
            uint identifier = ent.Identifier;
            uint generation = ent.Generation + 1;
            _entities[identifier] = new Entity { Identifier = _next_available.Identifier, Generation = generation };
            _next_available = new Entity { Identifier = identifier, Generation = 0 };
        }

        public void Replace<Component>(Entity ent, ref Component value) where Component : unmanaged
        {
            MemoryPool<Component> pool = GetPool<Component>();
            pool.Replace(ent, ref value);
        }

        public void Reserve(uint newCapacity)
        {
            _entities.Reserve(newCapacity);
        }

        public void Reserve<Component>(uint newCapacity) where Component : unmanaged
        {
            GetPool<Component>().Reserve(newCapacity);
        }

        public ComponentView<Component> GetView<Component>() where Component : unmanaged
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return new ComponentView<Component>(ref pool);
        }

        private Entity CreateNewIdentifier()
        {
            uint identifier = _entities.Count;
            uint generation = 0;
            Entity e = new Entity { Generation = generation, Identifier = identifier };
            _entities.Add(e);
            return e;
        }

        private Entity RecycleIdentifier()
        {
            uint identifier = _next_available.Identifier;
            uint generation = _entities[identifier].Generation;
            _next_available = _entities[identifier];
            return _entities[identifier] = new Entity { Generation = generation, Identifier = identifier };
        }

        private MemoryPool<Type> GetPool<Type>() where Type : unmanaged
        {
            Guid typeId = typeof(Type).GUID;
            if (_pools.ContainsKey(typeId))
            {
                return (MemoryPool<Type>)_pools[typeId];
            }

            MemoryPool<Type> pool = new MemoryPool<Type>();
            return (MemoryPool<Type>)(_pools[typeId] = pool);
        }
    }
}
