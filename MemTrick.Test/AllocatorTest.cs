using MemTrick.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

namespace MemTrick.Test
{
    [TestClass]
    public sealed class AllocatorTest : TestBase
    {
        private class DummyClass
        {
            public const int UninitializedIntField = 0;
            public const int DefaultConstructorIntField = 0xCC;

            public readonly int IntField;

            public DummyClass()
            {
                IntField = DefaultConstructorIntField;
            }
            private DummyClass(int intField)
            {
                IntField = intField;
            }

            public override int GetHashCode()
            {
                return IntField;
            }
            public override bool Equals(object obj)
            {
                if (obj is DummyClass)
                    return this.IntField == (obj as DummyClass).IntField;

                return false;
            }
        }

        [TestMethod]
        public void UninitializedAllocationTest()
        {
            Object original = FormatterServices.GetUninitializedObject(typeof(DummyClass));

            using (MemoryRestrictor.StartNoAlloc())
            {
                using (ClassAllocator.UninitializedAllocation(out DummyClass uheapObj))
                {
                    MemoryRestrictor.EndNoAlloc();

                    Assert.AreEqual(DummyClass.UninitializedIntField, uheapObj.IntField);
                    Assert.AreEqual(original, uheapObj);
                }
            }
        }

        [TestMethod]
        // TODO: Fix this to make no memory allocations.
        public void PublicConstructorCallTest()
        {
            ClassAllocator.PreCacheConstructor<DummyClass>();

            //using (MemoryRestrictor.StartNoAlloc())
            using (ClassAllocator.Allocate(out DummyClass uheapObj))
            {
                //MemoryRestrictor.EndNoAlloc();

                Assert.AreEqual(DummyClass.DefaultConstructorIntField, uheapObj.IntField);
            }
        }

        [TestMethod]
        // TODO: Fix this to make no memory allocations.
        public void PrivateConstructorCallTest()
        {
            const int expectedIntFieldValue = 0x34;

            //using (MemoryRestrictor.StartNoAlloc())
            using (ClassAllocator.Allocate(out DummyClass uheapObj, expectedIntFieldValue))
            {
                //MemoryRestrictor.EndNoAlloc();

                Assert.AreEqual(expectedIntFieldValue, uheapObj.IntField);
            }
        }
    }
}
