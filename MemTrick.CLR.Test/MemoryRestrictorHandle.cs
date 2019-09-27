using System;

namespace MemTrick.CLR.Test
{
    public struct MemoryRestrictorHandle : IDisposable
    {
        public void Dispose()
        {
            MemoryRestrictor.EndNoAlloc();
        }
    }
}
