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
            int align = sizeof(IntPtr) - 1;
            return (size + align) & (~align);
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
        /// See: ../../MemTrick/ImplementationMemo/String-Size.md
        /// </summary>
        [TestMethod]
        public void StringSize()
        {
            MethodTable* stringMt = MethodTable.GetMethodTable<String>();
            Assert.IsTrue(stringMt->HasComponentSize());
            Assert.AreEqual(
                sizeof(Char),
                stringMt->ComponentSize);

            // Temporary disabled until we implement allocation-free string.
            //Assert.AreEqual(
            //    AlignSizeByPointerSize(sizeof(ObjectHeader) + sizeof(Int32)) + sizeof(Char),
            //    stringMt->BaseSize);
        }
    }
}
