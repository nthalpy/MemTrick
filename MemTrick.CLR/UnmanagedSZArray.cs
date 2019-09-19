using MemTrick.CLR.RumtimeSpecific;
using System;

namespace MemTrick.CLR
{
    /// <summary>
    /// Unmanaged array with element type T.
    /// Do not allocate this with default constructor; use static method UnmanagedSZArray`1.Create(int).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal unsafe struct UnmanagedSZArray<T> : IDisposable
    {
        public static UnmanagedSZArray<T> Create(int size)
        {
            MethodTable* pElementMT = MethodTable.GetMethodTable<T>();
            MethodTable* pArrayMT = MethodTable.GetArrayMethodTable<T>();
            int elementSize = pElementMT->IsClass ? sizeof(IntPtr) : pElementMT->DataSize;

            void* addr = RawMemoryAllocator.Allocate(sizeof(ObjectHeader) + sizeof(SZArrayHeader) + elementSize * size);
            ObjectHeader* objHeader = (ObjectHeader*)addr;
            SZArrayHeader* szArrayHeader = (SZArrayHeader*)(objHeader + 1);

            objHeader->MethodTable = pArrayMT;
            szArrayHeader->NumComponents = size;

            void* objPtr = &(objHeader->MethodTable);
            PublicTypedReference tr = new PublicTypedReference
            {
                Value = &objPtr,
                Type = typeof(T[]).TypeHandle.Value.ToPointer(),
            };

            return new UnmanagedSZArray<T>(__refvalue(*(TypedReference*)&tr, T[]), addr);
        }

        public readonly T[] Array;
        private readonly void* address;

        private UnmanagedSZArray(T[] array, void* address)
        {
            Array = array;
            this.address = address;
        }

        public void Dispose()
        {
            if (address != null)
                RawMemoryAllocator.Free(address);
        }
    }
}
