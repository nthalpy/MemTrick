using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ParamTypeDesc
    {
        public TypeDesc Parent;

        private UInt64 templateMT;
        public TypeHandle Arg;
        private IntPtr exposedClassObject;

        internal MethodTable* GetTemplateMethodTableInternal()
        {
            fixed (ParamTypeDesc* pThis = &this)
                return (MethodTable*)((Byte*)&pThis->templateMT + templateMT);
        }
    }
}
