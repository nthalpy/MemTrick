﻿using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    /// <summary>
    /// Unmanaged array with element type T.
    /// Please do not allocate this with default constructor; use static method UnmanagedSZArray`1.Create(int).
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

            T[] arr = TypedReferenceHelper.PointerToObject<T[]>(objHeader);
            return new UnmanagedSZArray<T>(arr, addr);
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
