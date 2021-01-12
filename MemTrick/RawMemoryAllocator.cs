using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemTrick
{
    internal unsafe static class RawMemoryAllocator
    {
        // IMPORTANT:
        // Allocating class instances to arbitrary memory can 
        // interact w/ memory barrier related code, which can crash the runtime.
        //
        // Therefore, this code allocates inside the LOH instance.
        // @Harnel

        internal class AllocationEntry : IDisposable
        {
            // 4MB per entry.
            private const int EntrySize = 32 * 1024 * 1024;
            private struct EmptySlot
            {
                public int NextIndex;
            }

            public readonly IntPtr Base;

            private readonly Byte[] heap;
            private readonly int elementSize;
            private readonly GCHandle handle;

            private int head;
            private int tail;

            private int maxIndex
            {
                get
                {
                    return EntrySize / elementSize;
                }
            }

            public AllocationEntry(int elementSize)
            {
                this.elementSize = elementSize;
                head = 0;
                tail = maxIndex;

                // heap variable will be allocated in LOH,
                // and pinned until free AllocationEntry instance.
                heap = new Byte[EntrySize];
                handle = GCHandle.Alloc(heap, GCHandleType.Pinned);

                Base = handle.AddrOfPinnedObject();

                Initialize();
            }

            private void Initialize()
            {
                for (int idx = 0; idx < maxIndex; idx++)
                    ((EmptySlot*)GetAddressAtIndex(idx))->NextIndex = idx + 1;
            }

            private int GetIndexFromAddress(void* ptr)
            {
                return (Int32)((UInt64)ptr - (UInt64)Base) / elementSize;
            }
            private void* GetAddressAtIndex(int index)
            {
                return (Byte*)Base + index * elementSize;
            }

            public void* GetSlotAndAdvance()
            {
                // TODO:
                // Temporary exception handling code.
                // Should make more AllocationEntry, not throwing OOM.
                // @Harnel
                if (head == tail)
                    throw new OutOfMemoryException();

                void* rv = GetAddressAtIndex(head);

                head = ((EmptySlot*)rv)->NextIndex;

                FillMemory(rv, 0, elementSize);
                return rv;
            }
            public void Free(void* ptr)
            {
                int newtail = GetIndexFromAddress(ptr);

                ((EmptySlot*)GetAddressAtIndex(tail))->NextIndex = newtail;
                ((EmptySlot*)ptr)->NextIndex = 0xDEAD; // magic number for dead end
                tail = newtail;
            }

            public void Dispose()
            {
                handle.Free();
            }
        }

        private static readonly Dictionary<int, AllocationEntry> byteArrayList;

        static RawMemoryAllocator()
        {
            byteArrayList = new Dictionary<int, AllocationEntry>();
        }

        [Conditional("DEBUG")]
        private static void InsertAllocationInfo(int size, void* location)
        {
        }
        [Conditional("DEBUG")]
        private static void RemoveAllocationInfo(void* location)
        {
        }

        public static void MemCpy(void* dst, void* src, int size)
        {
            Byte* d = (Byte*)dst;
            Byte* s = (Byte*)src;

            for (int idx = 0; idx < size; idx++)
                *d++ = *s++;
        }

        public static unsafe void FillMemory(IntPtr dst, Byte c, int byteCount)
        {
            FillMemory((void*)dst, c, byteCount);
        }
        public static unsafe void FillMemory(void* dst, Byte c, int byteCount)
        {
            Byte* d = (Byte*)dst;

            for (int idx = 0; idx < byteCount; idx++)
                *d++ = c;
        }

        public static void* Allocate(int size)
        {
            if (byteArrayList.ContainsKey(size) == false)
                byteArrayList[size] = new AllocationEntry(size);

            return byteArrayList[size].GetSlotAndAdvance();
        }

        public static void* Reallocate(void* prev, int size)
        {
            throw new NotImplementedException();
        }

        public static void Free(int size, void* ptr)
        {
            byteArrayList[size].Free(ptr);
        }
    }
}
