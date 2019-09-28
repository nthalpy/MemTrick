using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemTrick.CLR
{
    public unsafe static class NewHijacker
    {
        [DllImport("Kernel32.dll")]
        private static extern bool VirtualProtectEx(
            IntPtr hProcess, IntPtr lpAddress,
            UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        private static CrawlResult AllocatorCrawlResult;
        private static UInt64 Backup;

        private static Action<IntPtr> PreAllocate;

        // Start of hijack.
        public static void Hijack(Action<IntPtr> preAllocate)
        {
            PreAllocate = preAllocate;

            AllocatorCrawlResult = MemoryCrawler.FindAllocator();

            CopyAllocator();
            HijackAllocator();
        }

        private static void CopyAllocator()
        {
            RuntimeMethodHandle mh = typeof(NewHijacker)
                .GetMethod(nameof(NewHijacker.OriginalAllocator), BindingFlags.NonPublic | BindingFlags.Static)
                .MethodHandle;
            RuntimeHelpers.PrepareMethod(mh);

            void* jitted = *((void**)mh.Value + 1);
            if (*(Byte*)jitted == 0xE9) // is this jump stub?
                jitted = (Byte*)jitted + *(Int32*)((Byte*)jitted + 1) + 5;

            Byte* src = (Byte*)AllocatorCrawlResult.Location;
            Byte* dst = (Byte*)jitted;

            for (int idx = 0; idx < AllocatorCrawlResult.Pattern.Size; idx++)
                dst[idx] = src[idx];

            // mov r11d, rel32나 jmp rel32의 값을 변경해줘야 한다.
            // mov r11d, dword ptr [coreclr!_tls_index]와 jmp JIT_New가 그 대상이다.
            checked
            {
                int adjust = (int)(src - dst);

                foreach (int offset in AllocatorCrawlResult.Pattern.ExternalRel32Offset)
                    *(Int32*)(dst + offset) += adjust;
            }
        }
        private static void HijackAllocator()
        {
            RuntimeMethodHandle mh = typeof(NewHijacker)
                .GetMethod(nameof(NewHijacker.Allocator), BindingFlags.NonPublic | BindingFlags.Static)
                .MethodHandle;
            void* precode = *((void**)mh.Value + 1);
            void* originalAllocator = AllocatorCrawlResult.Location;

            // jmp rel32
            UInt64 jmp = 0xE9;
            *(Int32*)((Byte*)&jmp + 1) = checked((Int32)((Int64)precode - (Int64)originalAllocator - 5));

            uint newProtect = 0x40; // execute, read, write
            uint oldProtect;
            VirtualProtectEx(
                Process.GetCurrentProcess().Handle, new IntPtr(originalAllocator),
                new UIntPtr(8), newProtect, out oldProtect);

            // Patch jump instruction
            Backup = *(UInt64*)originalAllocator;
            *(UInt64*)(originalAllocator) = jmp;
        }
        public static void Restore()
        {
            *(UInt64*)(AllocatorCrawlResult.Location) = Backup;
        }

        // Important:
        // Please don't try to allocate something in this method with `new`.
        // Using `new` in this method will throw StackOverflowException.
        private static Object Allocator(IntPtr mt)
        {
            PreAllocate.Invoke(mt);
            return OriginalAllocator(mt);
        }

        // Important:
        // This method contains no meaning, but has enough space to contain original coreclr allocator.
        private static Object OriginalAllocator(IntPtr mt)
        {
            switch (mt.ToInt32())
            {
                case 1:
                    throw new ArgumentException();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
