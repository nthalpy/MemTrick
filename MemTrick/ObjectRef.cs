using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    /// <summary>
    /// Value type object to retrieve reference type object.
    /// </summary>
    internal struct ObjectRef
    {
        private unsafe MethodTable** ptr;

        public unsafe int SyncBlock
        {
            get
            {
                return *(int*)(ptr - 1);
            }
            set
            {
                *(int*)(ptr - 1) = value;
            }
        }

        public unsafe MethodTable* MethodTablePtr
        {
            get
            {
                return *ptr;
            }
            set
            {
                *ptr = value;
            }
        }

        public unsafe void* ClassDataStartPoint
        {
            get
            {
                return ptr + 1;
            }
        }

        public unsafe ObjectRef(ObjectHeader* objectHeader)
        {
            ptr = (MethodTable**)objectHeader + 1;
        }

        public T AsClassType<T>() where T : class
        {
            unsafe
            {
                MethodTable** p = ptr;
                PublicTypedReference typedReference = new PublicTypedReference
                {
                    Value = &p,
                    Type = MethodTable.GetMethodTable<T>(),
                };

                return __refvalue(*(TypedReference*)&typedReference, T);
            }
        }
    }
}
