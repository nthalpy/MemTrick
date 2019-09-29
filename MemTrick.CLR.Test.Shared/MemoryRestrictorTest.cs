using MemTrick.CLR.Test.Exceptions;
using MemTrick.CLR.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public sealed class MemoryRestrictorTest : TestBase
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
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                Object obj = new Object();

                // Need to use obj variable to prevent being optimized out.
                Assert.IsNotNull(obj);
            }
        }
    }
}
