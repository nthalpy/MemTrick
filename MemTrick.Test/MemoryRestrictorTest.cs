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
        [ExpectedException(typeof(MemoryRestrictorException))]
        public void ThrowOnBoxing()
        {
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                int intValue = 0x123123;
                Object boxed = (Object)intValue;

                MemoryRestrictor.EndNoAlloc();
                Assert.IsTrue(boxed.ToString() == intValue.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MemoryRestrictorException))]
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
