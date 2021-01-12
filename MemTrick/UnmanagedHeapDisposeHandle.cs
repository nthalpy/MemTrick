using System;

namespace MemTrick
{
    /// <summary>
    /// Handle for disposing unmanaged heap memory. Use with using clause.
    /// </summary>
    public struct UnmanagedHeapDisposeHandle : IDisposable
    {
        private int size;
        private IntPtr objHeader;

        internal unsafe UnmanagedHeapDisposeHandle(int size, void* objHeader)
        {
            this.size = size;
            this.objHeader = (IntPtr)objHeader;
        }

        public void Dispose()
        {
            unsafe
            {
                RawMemoryAllocator.Free(size, (void*)objHeader);
            }
        }
    }
}
