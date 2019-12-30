using MemTrick.RumtimeSpecific;
using System;
using System.Runtime.InteropServices;

namespace MemTrick
{
    /// <summary>
    /// Helper class to avoid direct use of __makeref, __refvalue, and TypedReference struct.
    /// </summary>
    internal static class TypedReferenceHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct PublicTypedReference
        {
            public void* Value;
            public void* Type;
        }

        public unsafe static T PointerToObject<T>(ObjectHeader* objectHeader)
        {
            MethodTable** ppMT = &(objectHeader->MethodTable);
            PublicTypedReference typedReference = new PublicTypedReference
            {
                Value = &ppMT,
                Type = MethodTable.GetMethodTable<T>(),
            };

            return __refvalue(*(TypedReference*)&typedReference, T);
        }

        public unsafe static ObjectHeader* ClassToPointer<T>(T obj) where T : class
        {
            TypedReference tr = __makeref(obj);

            void* pMT = *(void**)((PublicTypedReference*)&tr)->Value;
            return (ObjectHeader*)(((Byte*)pMT) - sizeof(void*));
        }
        public unsafe static void* StructToPointer<T>(ref T obj) where T : struct
        {
            TypedReference tr = __makeref(obj);

            return ((PublicTypedReference*)&tr)->Value;
        }
    }
}
