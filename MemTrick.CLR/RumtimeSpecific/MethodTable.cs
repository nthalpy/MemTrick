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

        public unsafe static MethodTable* GetArrayMethodTable<TElem>()
        {
            TElem[] emptyArray = Array.Empty<TElem>();
            TypedReference tr = __makeref(emptyArray);

            return ***(MethodTable****)&tr;
        }

        [FieldOffset(0)]
        public Int32 Flags;

        [FieldOffset(0)]
        public Int16 ComponentSize;

        [FieldOffset(4)]
        public Int32 BaseSize;

        public unsafe int DataSize
        {
            get
            {
                return BaseSize - sizeof(ObjectHeader);
            }
        }
        
        /// <summary>
        /// We need this property because Type.IsClass is extremely slow.
        /// </summary>
        public bool IsClass
        {
            get
            {
                // 0x000C0000 means enum_flag_Category_ValueType_Mask
                return (Flags | 0x000C0000) == 0;
            }
        }

        public bool HasComponentSize()
        {
            // 0x80000000 means enum_flag_HasComponentSize
            return (Flags & 0x80000000) != 0;
        }
    }
}
