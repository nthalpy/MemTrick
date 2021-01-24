using MemTrick.RawMemory;
using System;

namespace MemTrick
{
    /// <summary>
    /// Handle for disposing memory. Use with using clause.
    /// </summary>
    public struct ClassDisposeHandle : IDisposable
    {
        public readonly IntPtr ObjHeader;
        private int size;

        internal unsafe ClassDisposeHandle(int size, void* objHeader)
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
