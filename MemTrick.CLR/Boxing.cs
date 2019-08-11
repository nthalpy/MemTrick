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
            void* srcBase = *(IntPtr**)&tr;

            objRef.SyncBlock = 0;
            objRef.MethodTablePtr = mt;
            void* dstBase = objRef.DataStartPoint;

            for (int idx = 0; idx < (mt->BaseSize - sizeof(ObjectHeader)) / 4; idx++)
                *((Int32*)dstBase + idx) = *((Int32*)srcBase + idx);

            return objRef;
        }
    }
}
