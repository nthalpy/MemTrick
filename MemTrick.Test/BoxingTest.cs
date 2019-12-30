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
            using (NoAllocFinalizer _ = MemoryRestrictor.StartNoAlloc())
            {
                int val = 0x12345678;

                using (BoxingHelper.Box(val, out Object boxed))
                {
                    MemoryRestrictor.EndNoAlloc();
                    Assert.AreEqual(boxed, val);
                }
            }
        }
    }
}
