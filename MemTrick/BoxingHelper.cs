using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    /// <summary>
    /// Boxing helper class.
    /// Supports methods for boxing without managed heap allocation.
    /// </summary>
    public static class BoxingHelper
    {
        public unsafe static UnmanagedHeapDisposeHandle Box<T>(T val, out Object boxed) where T : struct
        {
            MethodTable* mt = MethodTable.GetMethodTable<T>();
            int size = mt->BaseSize;

            void* p = RawMemoryAllocator.Allocate(size);
            ObjectRef objRef = new ObjectRef((ObjectHeader*)p);

            TypedReference tr = __makeref(val);
            void* src = *(IntPtr**)&tr;

            objRef.SyncBlock = 0;
            objRef.MethodTablePtr = mt;
            RawMemoryAllocator.MemCpy(objRef.ClassDataStartPoint, src, mt->DataSize);

            boxed = objRef.AsClassType<Object>();
            return new UnmanagedHeapDisposeHandle(p); 
        }
    }
}
