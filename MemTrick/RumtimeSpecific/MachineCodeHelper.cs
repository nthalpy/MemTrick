using System;

namespace MemTrick.RumtimeSpecific
{
    internal unsafe static class MachineCodeHelper
    {
        public static void AdjustExternalRel32(void* fnPtr, int size, int adjust)
        {
            Byte* p = (Byte*)fnPtr;

            // TODO: Temporary code. Need to parse assembly in fnPtr, and patch it.
            bool patched = false;

            if (*(p + 0x24) == 0xE9)
            {
                *(Int32*)(p + 0x24 + 1) += adjust;
                patched = true;
            }
            if (*(p + 0x1B) == 0xE9)
            {
                *(Int32*)(p + 0x1B + 1) += adjust;
                patched = true;
            }

            if (patched == false)
                throw new NotSupportedException("Failed to patch!");
        }
    }
}
