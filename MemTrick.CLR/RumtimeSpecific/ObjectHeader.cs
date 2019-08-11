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
        public MethodTable* MethodTable;
    }
}
