using MemTrick.RumtimeSpecific;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemTrick.Hijacking
{
    public unsafe static class NewHijacker
    {
        [DllImport("Kernel32.dll")]
        private static extern bool VirtualProtectEx(
            IntPtr hProcess, IntPtr lpAddress,
            UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        private static void* AllocatorLocation;
        private static UInt64 Backup;

        private static Action<IntPtr> PreAllocate;

        // Start of hijack.
        public static void Hijack(Action<IntPtr> preAllocate)
        {
            PreAllocate = preAllocate;
            AllocatorLocation = MemoryCrawler.FindAllocator();

            CopyAllocator();
            HijackAllocator();
        }

        private static void CopyAllocator()
        {
            RuntimeMethodHandle mh = typeof(NewHijacker)
                .GetMethod(nameof(NewHijacker.OriginalAllocator), BindingFlags.NonPublic | BindingFlags.Static)
                .MethodHandle;
            RuntimeHelpers.PrepareMethod(mh);

            void* jitted = *(void**)((Byte*)mh.Value + 8);
            if (*(Byte*)jitted == 0xE9) // is this jump stub?
                jitted = (Byte*)jitted + *(Int32*)((Byte*)jitted + 1) + 5;

            Byte* src = (Byte*)AllocatorLocation;
            Byte* dst = (Byte*)jitted;

            // 0x60 should be enough.
            for (int idx = 0; idx < 0x60; idx++)
                dst[idx] = src[idx];

            // scan whold method, adjust external rel32 addresses.
            checked
            {
                int adjust = (int)(src - dst);
                MachineCodeHelper.AdjustExternalRel32(dst, 0x60, adjust);
            }
        }
        private static void HijackAllocator()
        {
            RuntimeMethodHandle mh = typeof(NewHijacker)
                .GetMethod(nameof(NewHijacker.Allocator), BindingFlags.NonPublic | BindingFlags.Static)
                .MethodHandle;
            void* precode = *(void**)((Byte*)mh.Value + 8);
            void* originalAllocator = AllocatorLocation;

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
            *(UInt64*)AllocatorLocation = Backup;
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
