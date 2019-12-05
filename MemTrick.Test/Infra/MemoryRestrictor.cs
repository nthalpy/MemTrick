﻿using MemTrick.Hijacking;
using MemTrick.Test.Exceptions;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MemTrick.Test.Infra
{
    /// <summary>
    /// Hijacks new operator (usually JIT_TrialAllocSFastMP_InlineGetThread), and observes allocations.
    /// </summary>
    internal static class MemoryRestrictor
    {
        [ThreadStatic]
        private static bool IsNoAlloc;

        private static HijackFuncContext<IntPtr, Object> NewOperatorHijackContext;
        private static HijackFuncContext<Int32, String> FastAllocateStringHijackContext;

        /// <summary>
        /// Hijacks new operator. Hijacked new operator should be restored before AppDomain being unloaded.
        /// </summary>
        public static unsafe void Hijack()
        {
            BindingFlags bindingFlag = BindingFlags.NonPublic | BindingFlags.Static;

            NewOperatorHijackContext = new HijackFuncContext<IntPtr, Object>();
            NewOperatorHijackContext.MigrateInstruction += NewOperatorMigrateInstruction;
            MethodInfo newOperatorHook = typeof(MemoryRestrictor).GetMethod(nameof(NewOperatorHook), bindingFlag);
            HijackHelper.HijackUnmanagedMethod(MemoryCrawler.FindAllocator(), newOperatorHook, NewOperatorHijackContext);

            //FastAllocateStringHijackContext = new HijackFuncContext<Int32, String>();
            //FastAllocateStringHijackContext.MigrateInstruction += FastAllocateStringMigrateInstruction;
            //MethodInfo fastAllocateStringHook = typeof(MemoryRestrictor).GetMethod(nameof(FastAllocateStringHook), bindingFlag);
            //MethodInfo fastAllocateStringMethodInfo = typeof(String).GetMethod("FastAllocateString", bindingFlag);
            //HijackHelper.HijackManagedMethod(fastAllocateStringMethodInfo, fastAllocateStringHook, FastAllocateStringHijackContext);
        }


        /// <summary>
        /// Restores new operator.
        /// </summary>
        public static unsafe void Restore()
        {
            BindingFlags bindingFlag = BindingFlags.NonPublic | BindingFlags.Static;
            MethodInfo fastAllocateStringMethodInfo = typeof(String).GetMethod("FastAllocateString", bindingFlag);

            HijackHelper.RestoreUnmanagedMethod(MemoryCrawler.FindAllocator(), NewOperatorHijackContext);
            HijackHelper.RestoreManagedMethod(fastAllocateStringMethodInfo, FastAllocateStringHijackContext);
        }

        private static unsafe MigrationResult NewOperatorMigrateInstruction(IntPtr src, IntPtr dst, Int32 minimumCount)
        {
            Byte* pSrc = (Byte*)src;
            Byte* pDst = (Byte*)dst;

            // Works on...
            // x86 .NET Framework 4.5 ~ 4.8
            if (
                pSrc[0] == 0x8B && pSrc[1] == 0x41 && pSrc[2] == 0x04 &&
                pSrc[3] == 0x64 && pSrc[4] == 0x8B && pSrc[5] == 0x15 &&
                pSrc[10] == 0x03 && pSrc[11] == 0x42 && pSrc[12] == 0x40 &&
                pSrc[13] == 0x3B && pSrc[14] == 0x42 && pSrc[15] == 0x44)
            {
                RawMemoryAllocator.MemCpy(pDst, pSrc, 16);
                return new MigrationResult(16, 16);
            }
            // Works on... 
            // x64 .NET Framework 4.5 ~ 4.8
            // x64 .NET Core 2.0 ~ 2.1
            else if (
                pSrc[0] == 0x8B && pSrc[1] == 0x51 && pSrc[2] == 0x04 &&
                pSrc[3] == 0x65 && pSrc[4] == 0x4C && pSrc[5] == 0x8B && pSrc[6] == 0x1C && pSrc[7] == 0x25 &&
                pSrc[12] == 0x4D && pSrc[13] == 0x8B && pSrc[14] == 0x53)
            {
                RawMemoryAllocator.MemCpy(pDst, pSrc, 16);
                return new MigrationResult(16, 16);
            }
            // Works on...
            // x64 .NET Core 2.1 ~
            if (
                pSrc[0] == 0x8B && pSrc[1] == 0x51 && pSrc[2] == 0x04 &&
                pSrc[3] == 0x44 && pSrc[4] == 0x8B && pSrc[5] == 0x1D &&
                pSrc[10] == 0x65 && pSrc[11] == 0x48 && pSrc[12] == 0x8B && pSrc[13] == 0x04 && pSrc[14] == 0x25)
            {
                int srcOffset = 0;
                int dstOffset = 0;

                // mov edx, dword ptr [rcx+4]
                RawMemoryAllocator.MemCpy(pDst, pSrc, 3);
                srcOffset += 3;
                dstOffset += 3;

                // mov r11d, dword ptr [_something rel32_]
                // since distance between src and dst is very large, we need to change instructions to
                // mov r11, _something rel32_ / mov r11d, dword ptr [r11]
                void* location = pSrc + 3 + 7 + *(Int32*)(pSrc + 6);
                pDst[dstOffset + 0] = 0x49;
                pDst[dstOffset + 1] = 0xBB;
                pDst[dstOffset + 10] = 0x45;
                pDst[dstOffset + 11] = 0x8B;
                pDst[dstOffset + 12] = 0x1B;
                *(void**)(pDst + dstOffset + 2) = location;

                srcOffset += 7;
                dstOffset += 13;

                // mov rax, qword ptr gs:[58h]
                RawMemoryAllocator.MemCpy(pDst + dstOffset, pSrc + srcOffset, 9);
                srcOffset += 9;
                dstOffset += 9;

                return new MigrationResult(srcOffset, dstOffset);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        private unsafe static MigrationResult FastAllocateStringMigrateInstruction(IntPtr src, IntPtr dst, int minimumCount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts NoAlloc region. 
        /// Every managed heap allocation in NoAlloc region will raise MemoryRestrictorException.
        /// </summary>
        public static NoAllocFinalizer StartNoAlloc()
        {
            IsNoAlloc = true;
            return new NoAllocFinalizer();
        }
        /// <summary>
        /// Finishes NoAlloc region.
        /// </summary>
        public static void EndNoAlloc()
        {
            IsNoAlloc = false;
        }

        private static Object NewOperatorHook(IntPtr mt)
        {
            if (IsNoAlloc)
            {
                try
                {
                    IsNoAlloc = false;
                    Thread curr = Thread.CurrentThread;

                    throw new MemoryRestrictorException(
                        $"Tried to allocate in NoAlloc region. Thread: ({curr.Name}, #{curr.ManagedThreadId}), MT: {mt.ToInt64():X}");
                }
                finally
                {
                    IsNoAlloc = true;
                }
            }

            return NewOperatorHijackContext.Funclet.Invoke(mt);
        }
        private static String FastAllocateStringHook(Int32 length)
        {
            if (IsNoAlloc)
            {
                try
                {
                    IsNoAlloc = false;
                    Thread curr = Thread.CurrentThread;

                    throw new MemoryRestrictorException(
                        $"Tried to allocate new string with length {length}. Thread: ({curr.Name}, #{curr.ManagedThreadId})");
                }
                finally
                {
                    IsNoAlloc = true;
                }
            }

            return FastAllocateStringHijackContext.Funclet.Invoke(length);
        }
    }
}
