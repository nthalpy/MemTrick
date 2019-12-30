using System;

namespace MemTrick
{
    /// <summary>
    /// Handle for disposing unmanaged heap memory. Use with using clause.
    /// </summary>
    public struct UnmanagedHeapDisposeHandle : IDisposable
    {
        private IntPtr objHeader;

        internal unsafe UnmanagedHeapDisposeHandle(void* objHeader)
        {
            this.objHeader = (IntPtr)objHeader;
        }

        public void Dispose()
        {
            unsafe
            {
                RawMemoryAllocator.Free((void*)objHeader);
            }
        }
    }
}
