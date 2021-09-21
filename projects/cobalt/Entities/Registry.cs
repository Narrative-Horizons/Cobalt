using Cobalt.Core;
using Cobalt.Entities.Components;
using Cobalt.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Cobalt.Entities
{
    public class Registry
    {
        private Dictionary<Guid, IMemoryPool> _pools = new Dictionary<Guid, IMemoryPool>();
        private List<Entity> _entities = new List<Entity>();
        private Entity _next_available = Entity.Invalid;

        public uint Capacity => (uint)_entities.Capacity;
        public uint Count => (uint)_entities.Count;

        public EventManager Events { get; private set; } = new EventManager();

        public uint Active()
        {
            uint inactive = 0;

            for (Entity current = _next_available; !current.IsInvalid; ++inactive)
            {
                uint idx = current.Identifier;
                current = _entities[(int)idx];
            }

            return (uint)_entities.Count - inactive;
        }

        public Entity Create()
        {
            Entity result = _next_available == Entity.Invalid ? CreateNewIdentifier() : RecycleIdentifier();
            Events.Dispatch(new EntitySpawnEvent(result, this));
            Assign(result, new DirtyComponent());
            return result;
        }

        public void Assign<Component>(Entity ent, Component value) where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            pool.Assign(ent, ref value);
            value.Owner = ent;
            value.Registry = this;
            
            (value as ScriptableComponent)?.OnInit();

            Events.Dispatch(new ComponentAddEvent<Component>(ent, this, value));
        }

        public void AssignOrReplace<Component>(Entity ent, Component value) where Component : BaseComponent
        {
            if (Has<Component>(ent))
            {
                Replace(ent, value);
            }
            else
            {
                Assign(ent, value);
            }
        }

        public Component Get<Component>(Entity ent) where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return pool.Get(ent);
        }

        public Component TryGet<Component>(Entity ent) where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return pool.TryGet(ent);
        }

        public bool Has<Component>(Entity ent) where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return pool.Contains(ent);
        }

        public bool Has(Entity ent, Type type)
        {
            IMemoryPool pool = GetPool(type);
            return pool.Contains(ent);
        }

        public void Release(Entity ent)
        {
            Events.Dispatch(new EntityReleaseEvent(ent, this));

            // Release components
            foreach (var pool in _pools.Values)
            {
                pool.Remove(ent);
            }

            // Recycle entity
            uint identifier = ent.Identifier;
            uint generation = ent.Generation + 1;
            _entities[(int)identifier] = new Entity { Identifier = _next_available.Identifier, Generation = generation };
            _next_available = new Entity { Identifier = identifier, Generation = 0 };
        }

        public void Remove<Component>(Entity ent)
        {
            bool removed = GetPool<Component>().Remove(ent);
            if (removed)
            {
                Events.Dispatch(new ComponentRemoveEvent<Component>(ent, this));
            }
        }

        public void Replace<Component>(Entity ent, Component value) where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            pool.Replace(ent, value);
            value.Owner = ent;
            value.Registry = this;

            (value as ScriptableComponent)?.OnInit();

            Events.Dispatch(new ComponentReplaceEvent<Component>(ent, this, value));
        }

        public void Reserve(uint newCapacity)
        {
            _entities.Capacity = (int)newCapacity;
        }

        public void Reserve<Component>(uint newCapacity) where Component : BaseComponent
        {
            GetPool<Component>().Reserve(newCapacity);
        }

        public ComponentView<Component> GetView<Component>() where Component : BaseComponent
        {
            MemoryPool<Component> pool = GetPool<Component>();
            return new ComponentView<Component>(pool);
        }

        public EntityView GetView()
        {
            return new EntityView(this);
        }

        private Entity CreateNewIdentifier()
        {
            uint identifier = (uint)_entities.Count;
            uint generation = 0;
            Entity e = new Entity { Generation = generation, Identifier = identifier };
            _entities.Add(e);
            return e;
        }

        private Entity RecycleIdentifier()
        {
            uint identifier = _next_available.Identifier;
            uint generation = _entities[(int)identifier].Generation;
            _next_available = _entities[(int)identifier];
            return _entities[(int)identifier] = new Entity { Generation = generation, Identifier = identifier };
        }

        private MemoryPool<Type> GetPool<Type>()
        {
            Guid typeId = typeof(Type).GUID;
            if (_pools.ContainsKey(typeId))
            {
                return (MemoryPool<Type>)_pools[typeId];
            }

            MemoryPool<Type> pool = new MemoryPool<Type>();
            return (MemoryPool<Type>)(_pools[typeId] = pool);
        }

        internal IMemoryPool GetPool(Type t)
        {
            Guid typeId = t.GUID;
            if (_pools.ContainsKey(typeId))
            {
                return _pools[typeId];
            }

            Type generic = typeof(MemoryPool<>);
            Type specialized = generic.MakeGenericType(t);
            IMemoryPool pool = (IMemoryPool)Activator.CreateInstance(specialized);

            return _pools[typeId] = pool;
        }

        internal List<Entity> GetEntities()
        {
            return _entities;
        }
    }
}
