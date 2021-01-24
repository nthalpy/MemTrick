using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MemTrick.RawMemory
{
    internal unsafe static class RawMemoryManager
    {
        // IMPORTANT:
        // Allocating class instances to arbitrary memory can 
        // interact w/ memory barrier related code, which can crash the runtime.
        //
        // Therefore, this code allocates inside the LOH instance.
        // @Harnel
        private static readonly Dictionary<int, AllocationEntry> entries;

        static RawMemoryManager()
        {
            entries = new Dictionary<int, AllocationEntry>();
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
            if (entries.ContainsKey(size) == false)
                entries[size] = new AllocationEntry(size);

            return entries[size].GetSlotAndAdvance();
        }

        public static void* Reallocate(void* prev, int size)
        {
            throw new NotImplementedException();
        }

        public static void Free(int size, void* ptr)
        {
            entries[size].Free(ptr);
        }
    }
}
