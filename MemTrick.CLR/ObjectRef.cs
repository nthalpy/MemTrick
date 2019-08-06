using MemTrick.CLR.RumtimeSpecific;
using System;

namespace MemTrick.CLR
{
    /// <summary>
    /// Value type object to retrieve reference type object.
    /// </summary>
    public unsafe struct ObjectRef<T> : IDisposable
    {
        internal ObjectHeader* ObjectHeaderPtr;

        internal ObjectRef(ObjectHeader* objectHeader)
        {
            ObjectHeaderPtr = objectHeader;
        }

        public unsafe Object GetObject()
        {
            MethodTable** bodyPointer = ObjectHeader.GetMethodTablePointer(ObjectHeaderPtr);
            PublicTypedReference typedReference = new PublicTypedReference
            {
                ObjectBodyPointerRef = &bodyPointer,
                MethodTablePointer = MethodTable.GetMethodTable<Object>(),
            };

            return __refvalue(*(TypedReference*)&typedReference, Object);
        }

        public void Dispose()
        {
            if (ObjectHeaderPtr != null)
            {
                RawMemoryAllocator.Free(ObjectHeaderPtr);
                ObjectHeaderPtr = null;
            }
        }
    }
}
