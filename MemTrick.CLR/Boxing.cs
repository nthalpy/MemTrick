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
        public unsafe static ObjectRef<T> Box<T>(T val) where T : struct
        {
            MethodTable* mt = MethodTable.GetMethodTable<T>();
            int size = mt->BaseSize;

            ObjectRef<T> objRef = new ObjectRef<T>((ObjectHeader*)RawMemoryAllocator.Allocate(size));
            ObjectHeader* objHeaderPtr = objRef.ObjectHeaderPtr;

            TypedReference tr = __makeref(val);
            void* srcBase = *(IntPtr**)&tr;

            objHeaderPtr->SyncBlock = 0;
            objHeaderPtr->MethodTable = mt;
            void* dstBase = objHeaderPtr + 1;

            for (int idx = 0; idx < (mt->BaseSize - sizeof(ObjectHeader)) / 4; idx++)
                *((Int32*)dstBase + idx) = *((Int32*)srcBase + idx);

            return objRef;
        }
    }
}
