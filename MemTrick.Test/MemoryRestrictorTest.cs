using MemTrick.Test.Exceptions;
using MemTrick.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MemTrick.Test
{
    [TestClass]
    public sealed class MemoryRestrictorTest : TestBase
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext tc)
        {
            try
            {
                MemoryRestrictor.Hijack();
            }
            catch
            {
                MemoryRestrictor.Restore();
                throw;
            }
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

                MemoryRestrictor.EndNoAlloc();
                // Need to use 'obj' variable to prevent being optimized out.
                Assert.IsNotNull(obj);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
        public void ThrowOnLinq()
        {
            int[] array = new int[] { 1, 2, 3, 4 };

            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                // Enumerable.Sum(this IEnumerable`1) extension method internally uses 
                // IEnumerable`1.GetEnumerator, and invoking IEnumerable`1.GetEnumerator
                // on array makes allocation. (See SZArrayHelper.GetEnumerator method)
                int sum = array.Sum();

                MemoryRestrictor.EndNoAlloc();
                Assert.AreEqual(sum, 10);
            }

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(MemoryRestrictorException))]
        // TODO: Make this test pass by hijacking method which assigned to
        // hlpDynamicFuncTable[CORINFO_HELP_BOX].
        public void ThrowOnBoxing()
        {
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                int intValue = 0x123123;

                MemoryRestrictor.EndNoAlloc();
                // Invoking Object.GetHashCode will box 'intValue' variable first,
                // Then invoke it with.
                Assert.IsTrue(intValue.GetHashCode() == intValue);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
        // TODO: Make this test pass by hijacking String.FastAllocateString method.
        public void ThrowOnStringConcatenation()
        {
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                String s = String.Concat("A", "B");

                MemoryRestrictor.EndNoAlloc();
                Assert.AreEqual(s.Length, 2);
            }
        }
    }
}
