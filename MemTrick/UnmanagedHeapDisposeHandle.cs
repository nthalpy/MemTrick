using MemTrick.RawMemory;
using System;

namespace MemTrick
{
    /// <summary>
    /// Handle for disposing unmanaged heap memory. Use with using clause.
    /// </summary>
    public struct UnmanagedHeapDisposeHandle : IDisposable
    {
        public readonly IntPtr ObjHeader;
        private int size;

        internal unsafe UnmanagedHeapDisposeHandle(int size, void* objHeader)
        {
            this.size = size;
            this.ObjHeader = (IntPtr)objHeader;
        }

        public void Dispose()
        {
            unsafe
            {
                RawMemoryManager.Free(size, (void*)ObjHeader);
            }
        }
    }
}
