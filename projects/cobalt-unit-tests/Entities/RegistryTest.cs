using Cobalt.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cobalt.Tests.Unit.Entities
{
    [TestClass]
    public class RegistryTest
    {
        [TestMethod]
        public void DefaultConstructor()
        {
            Registry registry = new Registry();
            Assert.AreEqual(0U, registry.Active());
        }
    }
}
