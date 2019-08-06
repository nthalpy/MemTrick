using MemTrick.CLR.RumtimeSpecific;
using System.Runtime.InteropServices;

namespace MemTrick.CLR
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct PublicTypedReference
    {
        public MethodTable*** ObjectBodyPointerRef;
        public MethodTable* MethodTablePointer;
    }
}
