using MemTrick.AllocationFreeObject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MemTrick.Test
{
    [TestClass]
    public sealed class AllocationFreeListTest : TestBase
    {
        [TestMethod]
        public void OperationTest()
        {
            const int maxCapacity = 2048;

            Random rd = new Random();
            List<int> list1 = new List<int>(maxCapacity);
            UnmanagedHeapAllocator.PreCacheConstructor<AllocationFreeList<int>>();

            using (UnmanagedHeapAllocator.Allocate<AllocationFreeList<int>>(out AllocationFreeList<int> list2))
            {
                for (int iter = 0; iter < 100000; iter++)
                {
                    switch (rd.Next() % 2)
                    {
                        case 0:
                            if (list1.Count == maxCapacity)
                                break;

                            int val = rd.Next();
                            list1.Add(val);
                            list2.Add(val);
                            break;

                        case 1:
                            if (list1.Count == 0)
                                break;

                            int index = rd.Next() % list1.Count;
                            list1.RemoveAt(index);
                            list2.RemoveAt(index);
                            break;
                    }

                    Assert.AreEqual(list1.Count, list2.Count);
                    for (int idx = 0; idx < list1.Count; idx++)
                        Assert.AreEqual(list1[idx], list2[idx]);
                }
            }
        }
    }
}
