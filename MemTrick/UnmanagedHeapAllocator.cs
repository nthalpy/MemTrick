using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    public static class UnmanagedHeapAllocator
    {
        public static UnmanagedHeapDisposeHandle AllocateClass<T>(out T result) where T : class
        {
            unsafe
            {
                MethodTable* mt = MethodTable.GetMethodTable<T>();
                int size = mt->BaseSize;

                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(size);

                objHeader->SyncBlock = 0;
                objHeader->MethodTable = mt;
                RawMemoryAllocator.FillMemory(objHeader+1, 0, size);

                result = TypedReferenceHelper.PointerToObject<T>(objHeader);
                return new UnmanagedHeapDisposeHandle(objHeader);
            }
        }

        public static UnmanagedHeapDisposeHandle Box<T>(T val, out Object boxed) where T : struct
        {
            unsafe
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
}
