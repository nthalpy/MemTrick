using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SZArrayHeader
    {
        public Int32 NumComponents;

#if _TARGET_64BIT_
        public Int32 Pad;
#endif
    }
}
