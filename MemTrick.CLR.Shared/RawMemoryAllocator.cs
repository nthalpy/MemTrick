using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemTrick.CLR
{
    internal unsafe static class RawMemoryAllocator
    {
        static RawMemoryAllocator()
        {
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
            int* d = (int*)dst;
            int* s = (int*)src;

            for (int idx = 0; idx < size; idx += 4)
                *d++ = *s++;
        }

        public static void* Allocate(int size)
        {
            void* location = Marshal.AllocHGlobal(size).ToPointer();
            InsertAllocationInfo(size, location);
            return location;
        }

        public static void* Reallocate(void* prev, int size)
        {
            RemoveAllocationInfo(prev);
            void* location = Marshal.ReAllocHGlobal((IntPtr)prev, (IntPtr)size).ToPointer();
            InsertAllocationInfo(size, location);
            return location;
        }

        public static void Free(void* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
            RemoveAllocationInfo(ptr);
        }
    }
}
