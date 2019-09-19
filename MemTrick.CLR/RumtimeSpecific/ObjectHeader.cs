using System.Runtime.InteropServices;

namespace MemTrick.CLR.RumtimeSpecific
{
    /// <summary>
    /// Pointer to sync block.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ObjectHeader
    {
        public static MethodTable** GetMethodTablePointer(ObjectHeader* headerPtr)
        {
            return (MethodTable**)((int*)headerPtr + 1);
        }

        public int SyncBlock;

#if _TARGET_64BIT_
        private int Pad;
#endif

        public MethodTable* MethodTable;
    }
}
