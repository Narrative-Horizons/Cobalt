using Cobalt.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cobalt.Tests.Unit.Entities
{
    public struct TestComponent
    {
        public uint id;
    }

    [TestClass]
    public class RegistryTest
    {
        [TestMethod]
        public void DefaultConstructor()
        {
            Registry registry = new Registry();

            Assert.AreEqual(0U, registry.Active());
        }

        [TestMethod]
        public void AddSingleEntity()
        {
            Registry registry = new Registry();
            registry.Create();

            Assert.AreEqual(1U, registry.Active());
        }

        [TestMethod]
        public void AddSingleEntityThenRelease()
        {
            Registry registry = new Registry();
            Entity ent = registry.Create();

            Assert.AreEqual(1U, registry.Active());

            registry.Release(ent);

            Assert.AreEqual(0U, registry.Active());
        }

        [TestMethod]
        public void AddSingleEntityWithComponent()
        {
            Registry registry = new Registry();
            Entity ent = registry.Create();
            TestComponent component = new TestComponent { id = 1 };
            registry.Assign(ent, ref component);

            Assert.IsTrue(registry.Has<TestComponent>(ent));
        }

        [TestMethod]
        public void AddMultipleEntities()
        {
            uint count = 1024;
            Registry registry = new Registry();
            
            for (uint i = 0; i < count; ++i)
            {
                registry.Create();
            }

            Assert.AreEqual(count, registry.Active());
        }

        [TestMethod]
        public void AddMultipleEntitiesReserved()
        {
            uint count = 1024;
            Registry registry = new Registry();
            registry.Reserve(count);

            for (uint i = 0; i < count; ++i)
            {
                registry.Create();
            }

            Assert.AreEqual(count, registry.Active());
        }

        [TestMethod]
        public void ComponentViewEmpty()
        {
            Registry registry = new Registry();
            ComponentView<int> view = registry.GetView<int>();

            Assert.AreEqual(0u, view.Count);

            uint count = 0;
            view.ForEach(c => count += 1);

            Assert.AreEqual(0u, count);
        }

        [TestMethod]
        public void ComponentViewAllEntities()
        {
            uint count = 1024;
            Registry registry = new Registry();
            registry.Reserve(count);

            for (uint i = 0; i < count; ++i)
            {
                Entity e = registry.Create();
                TestComponent component = new TestComponent { id = 1 };
                registry.Assign(e, ref component);
            }

            ComponentView<TestComponent> view = registry.GetView<TestComponent>();
            Assert.AreEqual(count, view.Count);

            uint itCount = 0;
            view.ForEach(c =>
            {
                itCount += 1;
                Assert.AreEqual(1u, c.id);
            });

            Assert.AreEqual(count, itCount);
        }
    }
}
