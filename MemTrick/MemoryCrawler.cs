using System;
using System.Diagnostics;

namespace MemTrick
{
    /// <summary>
    /// Crawls memory to find methods.
    /// </summary>
    internal unsafe static class MemoryCrawler
    {
        private static readonly IntPtr BaseAddress;
        private static readonly int ModuleMemorySize;

        private const String DacTableResourceName = "COREXTERNALDATAACCESSRESOURCE";
        private static IntPtr DacTable;

        static MemoryCrawler()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName == "clr.dll" || module.ModuleName == "coreclr.dll")
                {
                    BaseAddress = (IntPtr)module.BaseAddress;
                    ModuleMemorySize = module.ModuleMemorySize;
                }
            }

            if (BaseAddress == IntPtr.Zero)
                throw new Exception("Unable to find proper runtime module (clr/coreclr).");
        }

        public static void InitializeDac()
        {
            Byte* p = (Byte*)BaseAddress;

            for (int offset = 0; offset < ModuleMemorySize - DacTableResourceName.Length * 2; offset++)
            {
                bool matches = true;
                for (int idx = 0; idx < DacTableResourceName.Length; idx++)
                {
                    if ((p + offset)[2 * idx] != DacTableResourceName[idx]
                        || (p + offset)[2 * idx + 1] != 0)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    // TODO: Remove RuntimeSpecific.DacOffset magic number.
                    DacTable = (IntPtr)(p + offset + RuntimeSpecificConstant.DacOffset);
                    break;
                }
            }

            if (DacTable == IntPtr.Zero)
                throw new NotSupportedException("Unable to find DAC Table!");
        }

        public static void* RetrieveMethod(DynamicJitHelperEnum e)
        {
            if (DacTable == IntPtr.Zero)
                InitializeDac();

            // DacTable[8]: clr!hlpDynamicFuncTable
            void** hlpDynamicFuncTable = (void**)((Byte*)BaseAddress + ((UInt32*)DacTable)[8]);
            return hlpDynamicFuncTable[(int)e];
        }
    }
}
