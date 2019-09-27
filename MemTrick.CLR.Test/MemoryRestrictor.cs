using System;
using System.Threading;

namespace MemTrick.CLR.Test
{
    public static class MemoryRestrictor
    {
        [ThreadStatic]
        public static bool IsNoAlloc;

        public static void Hijack()
        {
            NewHijacker.Hijack(PreAllocate);
        }
        public static void Restore()
        {
            NewHijacker.Restore();
        }

        public static MemoryRestrictorHandle StartNoAlloc()
        {
            IsNoAlloc = true;
            return new MemoryRestrictorHandle();
        }
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
