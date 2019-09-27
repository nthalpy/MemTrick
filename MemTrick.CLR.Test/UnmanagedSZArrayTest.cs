using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public unsafe class UnmanagedSZArrayTest
    {
        [TestMethod]
        public void PrimitiveOperation()
        {
            Random rd = new Random();
            int size = rd.Next(1000, 2000);
            int[] original = new int[size];

            for (int idx = 0; idx < original.Length; idx++)
                original[idx] = rd.Next();

            bool result;
            using (MemoryRestrictorHandle h = MemoryRestrictor.StartNoAlloc())
            using (UnmanagedSZArray<int> intSZArray = UnmanagedSZArray<int>.Create(size))
            {
                int[] arr = intSZArray.Array;

                for (int idx = 0; idx < arr.Length; idx++)
                    arr[idx] = original[idx];

                // Note that we can't use IEnumerable`1.SequenceEquals, because it uses GetEnumerator, and
                // SZArrayHelper.GetEnumerator makes object in heap.
                result = true;
                result &= (arr.Length == original.Length);
                for (int idx = 0; idx < arr.Length; idx++)
                    result &= (arr[idx] == original[idx]);
            }

            Assert.IsTrue(result);
        }
    }
}
