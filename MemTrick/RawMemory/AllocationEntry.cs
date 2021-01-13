using System;
using System.Runtime.InteropServices;

namespace MemTrick.RawMemory
{
    internal unsafe class AllocationEntry : IDisposable
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

        private readonly int maxIndex;

        public AllocationEntry(int elementSize)
        {
            this.elementSize = elementSize;
            head = 0;
            maxIndex = EntrySize / elementSize;
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

            RawMemoryManager.FillMemory(rv, 0, elementSize);
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
}
