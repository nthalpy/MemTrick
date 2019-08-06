using System;
using System.Runtime.InteropServices;

namespace MemTrick.CLR.RumtimeSpecific
{
    /// <summary>
    /// This is copy of MethodTable object in runtime.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct MethodTable
    {
        public unsafe static MethodTable* GetMethodTable<T>()
        {
            return GetMethodTable(typeof(T));
        }
        public unsafe static MethodTable* GetMethodTable(Type t)
        {
            return (MethodTable*)t.TypeHandle.Value;
        }

        [FieldOffset(0)]
        public Int32 Flags;

        [FieldOffset(0)]
        public Int16 ComponentSize;

        [FieldOffset(4)]
        public Int32 BaseSize;

        public bool HasComponentSize()
        {
            return (Flags & 0x80000000) != 0;
        }
    }
}
