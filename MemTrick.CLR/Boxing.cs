using MemTrick.CLR.RumtimeSpecific;
using System;

namespace MemTrick.CLR
{
    /// <summary>
    /// Boxing helper class.
    /// Supports methods for boxing without managed heap allocation.
    /// </summary>
    public static class Boxing
    {
        public unsafe static ObjectRef Box<T>(T val) where T : struct
        {
            MethodTable* mt = MethodTable.GetMethodTable<T>();
            int size = mt->BaseSize;

            ObjectRef objRef = new ObjectRef((ObjectHeader*)RawMemoryAllocator.Allocate(size));

            TypedReference tr = __makeref(val);
            void* src = *(IntPtr**)&tr;

            objRef.SyncBlock = 0;
            objRef.MethodTablePtr = mt;
            void* dst = objRef.ClassDataStartPoint;

            RawMemoryAllocator.MemCpy(dst, src, mt->DataSize);

            return objRef;
        }
    }
}
