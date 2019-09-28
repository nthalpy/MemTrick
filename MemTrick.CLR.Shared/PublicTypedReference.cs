using MemTrick.CLR.RumtimeSpecific;
using System;
using System.Runtime.InteropServices;

namespace MemTrick.CLR
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct PublicTypedReference
    {
        public void* Value;
        public void* Type;

        public ObjectRef ToObjectRef()
        {
            return new ObjectRef((ObjectHeader*)((Byte*)Value - sizeof(void*)));
        }
    }
}
