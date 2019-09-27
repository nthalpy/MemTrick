using MemTrick.CLR.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
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

                using (ObjectRef objRef = Boxing.Box(val))
                {
                    Object boxed = objRef.GetObject();

                    MemoryRestrictor.EndNoAlloc();
                    Assert.AreEqual(boxed, val);
                }
            }
        }
    }
}
