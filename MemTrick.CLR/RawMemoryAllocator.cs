using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MemTrick.CLR
{
    internal unsafe static class RawMemoryAllocator
    {
        private struct AllocationInfo
        {
            public int Size;
            public void* Location;
        }

        private static Dictionary<IntPtr, AllocationInfo> allocationInfoDict;

        static RawMemoryAllocator()
        {
#if DEBUG
            allocationInfoDict = new Dictionary<IntPtr, AllocationInfo>(4096);
            Process.GetCurrentProcess().Exited += RawMemoryAllocator_Exited;
#endif
        }

        private static void RawMemoryAllocator_Exited(Object sender, EventArgs e)
        {
            if (allocationInfoDict.Count != 0)
            {
                StringBuilder errorMessageBuilder = new StringBuilder();
                errorMessageBuilder.AppendLine($"Raw memories are not disposed properly! Leaked memories are:");

                foreach (AllocationInfo info in allocationInfoDict.Values)
                    errorMessageBuilder.AppendLine($"\tSize: {info.Size}, Location: {new IntPtr(info.Location)}");

                throw new MemoryLeakException(errorMessageBuilder.ToString());
            }
        }

        [Conditional("DEBUG")]
        private static void InsertAllocationInfo(int size, void* location)
        {
            lock (allocationInfoDict)
            {
                allocationInfoDict.Add(
                    new IntPtr(location),
                    new AllocationInfo { Size = size, Location = location });
            }
        }
        [Conditional("DEBUG")]
        private static void RemoveAllocationInfo(void* location)
        {
            lock (allocationInfoDict)
            {
                IntPtr key = new IntPtr(location);

                if (allocationInfoDict.ContainsKey(key) == false)
                    throw new KeyNotFoundException($"Pointer {key} is not allocated with {nameof(RawMemoryAllocator)}.");

                allocationInfoDict.Remove(key);
            }
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
            void* location = Marshal.ReAllocHGlobal(new IntPtr(prev), new IntPtr(size)).ToPointer();
            InsertAllocationInfo(size, location);
            return location;
        }

        public static void Free(void* ptr)
        {
            Marshal.FreeHGlobal(new IntPtr(ptr));
            RemoveAllocationInfo(ptr);
        }
    }
}
