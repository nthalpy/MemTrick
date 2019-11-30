using System;
using System.Runtime.InteropServices;

namespace MemTrick
{
    internal static class Kernel32
    {
        public enum PageProtection : UInt32
        {
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Invalid = 0x40000000,
        }

        [DllImport("Kernel32.dll")]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize,
            PageProtection flNewProtect, out PageProtection lpflOldProtect);
    }
}
