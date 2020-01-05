using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TypeHandle
    {
        public IntPtr TAddr;
        
        public MethodTable* GetMethodTable()
        {
            if (IsTypeDesc())
                return AsTypeDesc()->GetMethodTable();
            else
                return AsMethodTable();
        }

        private bool IsTypeDesc()
        {
            return ((UInt64)TAddr & 2) != 0;
        }

        public MethodTable* AsMethodTable()
        {
            return (MethodTable*)TAddr;
        }
        public TypeDesc* AsTypeDesc()
        {
            return (TypeDesc*)(((Byte*)TAddr) - 2);
        }
    }
}
