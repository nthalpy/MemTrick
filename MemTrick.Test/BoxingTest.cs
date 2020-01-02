using MemTrick.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.Test
{
    [TestClass]
    public sealed class BoxingTest : TestBase
    {
        [TestMethod]
        public void Int32BoxingTest()
        {
            using (MemoryRestrictor.StartNoAlloc())
            {
                int val = 12345678;

                using (UnmanagedHeapAllocator.Box(val, out Object boxed))
                {
                    MemoryRestrictor.EndNoAlloc();
                    Assert.AreEqual(val, boxed);
                }
            }
        }
    }
}
