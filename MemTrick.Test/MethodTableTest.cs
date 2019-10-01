using MemTrick.RumtimeSpecific;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.Test
{
    [TestClass]
    public unsafe sealed class MethodTableTest : TestBase
    {
        private int AlignSizeByPointerSize(int size)
        {
            int pointerSize = sizeof(IntPtr);

            if (size % pointerSize == 0)
                return size;
            else
                return 1 + (size | (pointerSize - 1));
        }

        [TestMethod]
        public void ObjectHeaderSize()
        {
            Assert.AreEqual(sizeof(ObjectHeader), 2 * sizeof(IntPtr));
        }

        /// <summary>
        /// Compare base size field of method table with known values.
        /// </summary>
        [TestMethod]
        public void PrimitiveTypeSize()
        {
            MethodTable* byteMt = MethodTable.GetMethodTable<Byte>();
            Assert.AreEqual(
                AlignSizeByPointerSize(sizeof(ObjectHeader) + sizeof(Byte)),
                byteMt->BaseSize);

            MethodTable* intMt = MethodTable.GetMethodTable<Int32>();
            Assert.AreEqual(
                AlignSizeByPointerSize(sizeof(ObjectHeader) + sizeof(Int32)),
                intMt->BaseSize);

            MethodTable* doubleMt = MethodTable.GetMethodTable<Double>();
            Assert.AreEqual(
                AlignSizeByPointerSize(sizeof(ObjectHeader) + sizeof(Double)),
                doubleMt->BaseSize);
        }

        /// <summary>
        /// Compare base size and component size with known values.
        /// </summary>
        [TestMethod]
        public void StringSize()
        {
            MethodTable* stringMt = MethodTable.GetMethodTable<String>();
            Assert.IsTrue(stringMt->HasComponentSize());
            Assert.AreEqual(
                sizeof(Char),
                stringMt->ComponentSize);
            Assert.AreEqual(
                AlignSizeByPointerSize(sizeof(ObjectHeader) + sizeof(Int32)) + sizeof(Char),
                stringMt->BaseSize);
        }
    }
}
