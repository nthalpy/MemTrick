using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    public static class UnmanagedHeapAllocator
    {
        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static UnmanagedHeapDisposeHandle UninitializedAllocation<T>(out T result) where T : class
        {
            UnmanagedHeapDisposeHandle handle = UninitializedAllocation(typeof(T), out Object obj);
            result = obj as T;

            return handle;
        }

        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static UnmanagedHeapDisposeHandle UninitializedAllocation(Type t, out Object result)
        {
            unsafe
            {
                MethodTable* mt = MethodTable.GetMethodTable(t);
                int size = mt->BaseSize;

                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(size);

                objHeader->SyncBlock = 0;
                objHeader->MethodTable = mt;
                RawMemoryAllocator.FillMemory(objHeader + 1, 0, size - sizeof(ObjectHeader));

                result = TypedReferenceHelper.PointerToObject<Object>(objHeader);
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
