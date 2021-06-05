using Cobalt.Core;
using Cobalt.Entities;

namespace Cobalt.Events
{
    public class EntitySpawnEvent : EventData
    {
        public Entity SpawnedEntity { get; private set; }
        public Registry SpawningRegistry { get; private set; }

        public EntitySpawnEvent(Entity spawned, Registry spawner)
        {
            SpawnedEntity = spawned;
            SpawningRegistry = spawner;
        }
    }

    public class EntityReleaseEvent : EventData
    {
        public Entity SpawnedEntity { get; private set; }
        public Registry SpawningRegistry { get; private set; }

        public EntityReleaseEvent(Entity spawned, Registry spawner)
        {
            SpawnedEntity = spawned;
            SpawningRegistry = spawner;
        }
    }

    public class ComponentAddEvent<Type> : EventData
    {
        public Entity Entity { get; private set; }
        public Registry Registry { get; private set; }
        public Type Component { get; private set; }

        public ComponentAddEvent(Entity entity, Registry registry, Type component)
        {
            Entity = entity;
            Registry = registry;
            Component = component;
        }
    }

    public class ComponentReplaceEvent<Type> : EventData
    {
        public Entity Entity { get; private set; }
        public Registry Registry { get; private set; }
        public Type Component { get; private set; }

        public ComponentReplaceEvent(Entity entity, Registry registry, Type component)
        {
            Entity = entity;
            Registry = registry;
            Component = component;
        }
    }

    public class ComponentRemoveEvent<Type> : EventData
    {
        public Entity Entity { get; private set; }
        public Registry Registry { get; private set; }

        public ComponentRemoveEvent(Entity entity, Registry registry)
        {
            Entity = entity;
            Registry = registry;
        }
    }
}
