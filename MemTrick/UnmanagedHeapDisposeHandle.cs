using System;
using System.Collections.Generic;
using System.Text;

namespace MemTrick
{
    public struct UnmanagedHeapDisposeHandle : IDisposable
    {
        private IntPtr p;

        internal unsafe UnmanagedHeapDisposeHandle(void* p)
        {
            this.p = (IntPtr)p;
        }

        public void Dispose()
        {
            unsafe
            {
                RawMemoryAllocator.Free((void*)p);
            }
        }
    }
}
