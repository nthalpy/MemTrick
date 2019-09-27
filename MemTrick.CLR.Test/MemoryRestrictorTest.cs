using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public sealed class MemoryRestrictorTest
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext tc)
        {
            MemoryRestrictor.Hijack();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            MemoryRestrictor.Restore();
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
        public void TestAllocation()
        {
            using (MemoryRestrictorHandle handle = MemoryRestrictor.StartNoAlloc())
            {
                Assert.IsNotNull(new Object());
            }
        }
    }
}
