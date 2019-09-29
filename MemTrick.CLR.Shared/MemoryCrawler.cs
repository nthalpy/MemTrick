using System;
using System.Diagnostics;

namespace MemTrick.CLR
{
    /// <summary>
    /// Crawls memory to find methods.
    /// </summary>
    internal unsafe static class MemoryCrawler
    {
        private static readonly void* BaseAddress;
        private static readonly int ModuleMemorySize;

        private const String DacTableResourceName = "COREXTERNALDATAACCESSRESOURCE";
        private static UInt32* DacTable;

        static MemoryCrawler()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName == "clr.dll" || module.ModuleName == "coreclr.dll")
                {
                    BaseAddress = (void*)module.BaseAddress;
                    ModuleMemorySize = module.ModuleMemorySize;
                }
            }

            if (BaseAddress == null)
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
                    DacTable = (UInt32*)(p + offset + 0x78);
                    break;
                }
            }

            if (DacTable == null)
                throw new NotSupportedException("Unable to find DAC Table!");
        }

        public static void* FindAllocator()
        {
            if (DacTable == null)
                InitializeDac();

            // DacTable[8]: clr!hlpDynamicFuncTable
            void** hlpDynamicFuncTable = (void**)((Byte*)BaseAddress + DacTable[8]);

            // hlpDynamicFuncTable[3]: CORINFO_HELP_NEWSFAST
            return hlpDynamicFuncTable[3];
        }
    }
}
