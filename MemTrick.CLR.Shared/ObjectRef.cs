using MemTrick.CLR.RumtimeSpecific;
using System;

namespace MemTrick.CLR
{
    /// <summary>
    /// Value type object to retrieve reference type object.
    /// </summary>
    public unsafe struct ObjectRef : IDisposable
    {
        private MethodTable** ptr;

        internal int SyncBlock
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

        internal MethodTable* MethodTablePtr
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

        internal void* ClassDataStartPoint
        {
            get
            {
                return ptr + 1;
            }
        }
        internal void* StructDataStartPoint
        {
            get
            {
                return ptr;
            }
        }

        internal ObjectRef(ObjectHeader* objectHeader)
        {
            ptr = (MethodTable**)objectHeader + 1;
        }

        public unsafe Object GetObject()
        {
            return GetObject<Object>();
        }
        public unsafe T GetObject<T>()
        {
            MethodTable** p = ptr;
            PublicTypedReference typedReference = new PublicTypedReference
            {
                Value = &p,
                Type = MethodTable.GetMethodTable<T>(),
            };

            return __refvalue(*(TypedReference*)&typedReference, T);
        }

        public void Dispose()
        {
            if (ptr != null)
            {
                RawMemoryAllocator.Free(ptr - 1);
                ptr = null;
            }
        }
    }
}
