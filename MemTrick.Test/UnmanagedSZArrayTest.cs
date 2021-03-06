﻿using MemTrick.Test.Infra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MemTrick.Test
{
    [TestClass]
    public sealed class UnmanagedSZArrayTest : TestBase
    {
        [TestMethod]
        public void PrimitiveOperation()
        {
            Random rd = new Random();
            int size = rd.Next(1000, 2000);
            int[] original = new int[size];

            for (int idx = 0; idx < original.Length; idx++)
                original[idx] = rd.Next();

            using (MemoryRestrictor.StartNoAlloc())
            using (ClassAllocator.AllocateSZArray(size, out int[] arr))
            {
                for (int idx = 0; idx < arr.Length; idx++)
                    arr[idx] = original[idx];

                MemoryRestrictor.EndNoAlloc();
                Assert.IsTrue(arr.SequenceEqual(original));
            }
        }
    }
}
