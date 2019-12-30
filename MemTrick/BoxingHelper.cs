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

            ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(size);

            void* src = TypedReferenceHelper.StructToPointer(ref val);

            objHeader->SyncBlock = 0;
            objHeader->MethodTable = mt;
            RawMemoryAllocator.MemCpy(objHeader + 1, src, mt->DataSize);

            boxed = TypedReferenceHelper.PointerToObject<Object>(objHeader);
            return new UnmanagedHeapDisposeHandle(objHeader);
        }
    }
}
