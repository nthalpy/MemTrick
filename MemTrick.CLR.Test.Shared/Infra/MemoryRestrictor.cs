using MemTrick.CLR.Test.Exceptions;
using System;
using System.Threading;

namespace MemTrick.CLR.Test.Infra
{
    /// <summary>
    /// Hijacks new operator (usually JIT_TrialAllocSFastMP_InlineGetThread), and observes allocations.
    /// </summary>
    internal static class MemoryRestrictor
    {
        [ThreadStatic]
        private static bool IsNoAlloc;

        /// <summary>
        /// Hijacks new operator. Hijacked new operator should be restored before AppDomain being unloaded.
        /// </summary>
        public static void Hijack()
        {
            NewHijacker.Hijack(PreAllocate);
        }
        /// <summary>
        /// Restores new operator.
        /// </summary>
        public static void Restore()
        {
            NewHijacker.Restore();
        }

        /// <summary>
        /// Starts NoAlloc region. 
        /// Every managed heap allocation in NoAlloc region will raise MemoryRestrictorException.
        /// </summary>
        public static NoAllocFinalizer StartNoAlloc()
        {
            IsNoAlloc = true;
            return new NoAllocFinalizer();
        }
        /// <summary>
        /// Finishes NoAlloc region.
        /// </summary>
        public static void EndNoAlloc()
        {
            IsNoAlloc = false;
        }

        private static void PreAllocate(IntPtr obj)
        {
            if (IsNoAlloc)
            {
                try
                {
                    IsNoAlloc = false;
                    Thread curr = Thread.CurrentThread;

                    throw new MemoryRestrictorException(
                        $"Tried to allocate in NoAlloc region. Thread: ({curr.Name}, #{curr.ManagedThreadId}), MT: {obj.ToInt64():X}");
                }
                finally
                {
                    IsNoAlloc = true;
                }
            }
        }
    }
}
