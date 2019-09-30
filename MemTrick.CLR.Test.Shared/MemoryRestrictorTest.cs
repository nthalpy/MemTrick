using MemTrick.CLR.Test.Exceptions;
using MemTrick.CLR.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
        public void ThrowOnAllocation()
        {
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                Object obj = new Object();

                // Need to use 'obj' variable to prevent being optimized out.
                Assert.IsNotNull(obj);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
        public void ThrowOnLinq()
        {
            int[] array = new int[] { 1, 2, 3, 4 };
            int sum = 0;

            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                // Enumerable.Sum(this IEnumerable`1) extension method internally uses 
                // IEnumerable`1.GetEnumerator, and invoking IEnumerable`1.GetEnumerator
                // on array makes allocation. (See SZArrayHelper.GetEnumerator method)
                sum = array.Sum();
            }

            Assert.AreEqual(sum, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
        // TODO: Make this test pass by hijacking method which assigned to
        // hlpDynamicFuncTable[CORINFO_HELP_BOX].
        public void ThrowOnBoxing()
        {
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                int intValue = 0x123123;

                // Invoking Object.GetHashCode will box 'intValue' variable first,
                // Then invoke it with.
                Assert.IsTrue(intValue.GetHashCode() == intValue);
            }
        }
    }
}
