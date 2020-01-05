using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ParamTypeDesc
    {
        public TypeDesc Parent;

        private IntPtr templateMT;
        public TypeHandle Arg;
        private IntPtr exposedClassObject;

        internal MethodTable* GetTemplateMethodTableInternal()
        {
            // TODO: Separate this logic
            if ((UInt64)templateMT > 0x1000000)
            {
                // .NET Framework 4.5~4.8, .NET Core 2.0 ~ 2.0.9 uses this logic:
                return (MethodTable*)templateMT;
            }
            else
            {
                fixed (ParamTypeDesc* pThis = &this)
                    return (MethodTable*)((Byte*)&pThis->templateMT + (UInt64)templateMT);
            }
        }
    }
}
