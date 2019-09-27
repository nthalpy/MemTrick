using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public unsafe class BoxingTest
    {
        [TestMethod]
        public void Int32BoxingTest()
        {
            bool result;

            using (MemoryRestrictorHandle h = MemoryRestrictor.StartNoAlloc())
            {
                int val = 0x12345678;

                using (ObjectRef objRef = Boxing.Box(val))
                {
                    Object boxed = objRef.GetObject();
                    result = val.Equals(boxed);
                }
            }

            Assert.IsTrue(result);
        }
    }
}
