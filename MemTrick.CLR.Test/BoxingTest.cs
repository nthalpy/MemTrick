using MemTrick.CLR.RumtimeSpecific;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public unsafe class MethodTableTest
    {
        private int AlignSizeBy4(int size)
        {
            if (size % 4 == 0)
                return size;
            else
                return size + 4 - size % 4;
        }

        /// <summary>
        /// Compare base size field of method table with known values.
        /// </summary>
        [TestMethod]
        public void TestPrimitiveTypeSize()
        {
            MethodTable* byteMt = MethodTable.GetMethodTable<Byte>();
            Assert.AreEqual(
                AlignSizeBy4(sizeof(ObjectHeader) + sizeof(Byte)),
                byteMt->BaseSize);

            MethodTable* intMt = MethodTable.GetMethodTable<Int32>();
            Assert.AreEqual(
                sizeof(ObjectHeader) + sizeof(Int32),
                intMt->BaseSize);

            MethodTable* doubleMt = MethodTable.GetMethodTable<Double>();
            Assert.AreEqual(
                sizeof(ObjectHeader) + sizeof(Double),
                doubleMt->BaseSize);
        }

        /// <summary>
        /// Compare base size and component size with known values.
        /// </summary>
        [TestMethod]
        public void TestStringSize()
        {
            MethodTable* stringMt = MethodTable.GetMethodTable<String>();
            Assert.IsTrue(stringMt->HasComponentSize());
            Assert.AreEqual(
                sizeof(Char),
                stringMt->ComponentSize);
            Assert.AreEqual(
                sizeof(ObjectHeader) + sizeof(Int32) + sizeof(Char),
                stringMt->BaseSize);
        }

        [TestMethod]
        public void Int32BoxingTest()
        {
            int val = 0x12345678;

            using (ObjectRef<int> objRef = Boxing.Box(val))
            {
                Object boxed = objRef.GetObject();
                Assert.AreEqual(
                    val,
                    boxed);
            }
        }
    }
}
