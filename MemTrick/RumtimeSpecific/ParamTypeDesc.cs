using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ParamTypeDesc
    {
        public TypeDesc Parent;

        public MethodTable* TemplateMT;
        public TypeHandle Arg;
        public IntPtr ExposedClassObject;

        internal MethodTable* GetTemplateMethodTableInternal()
        {
            return TemplateMT;
        }
    }
}
