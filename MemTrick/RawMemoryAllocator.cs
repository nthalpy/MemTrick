using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemTrick
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
