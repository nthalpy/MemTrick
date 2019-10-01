using System;

namespace MemTrick.Test.Infra
{
    /// <summary>
    /// Use with "using clause" to invoke EndNoAlloc method automatically.
    /// </summary>
    internal struct NoAllocFinalizer : IDisposable
    {
        public void Dispose()
        {
            MemoryRestrictor.EndNoAlloc();
        }
    }
}
