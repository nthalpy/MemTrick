using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MemTrick.Hijacking
{
    public static class HijackHelper
    {
        // TODO: Extract all AMD64-related logics to other helper class

        public static unsafe void HijackManagedMethod(MethodInfo target, MethodInfo hook, HijackContextBase context)
        {
            // Make sure method is jitted already.
            RuntimeHelpers.PrepareMethod(hook.MethodHandle);

            Byte* pTargetMethod = (Byte*)target.MethodHandle.GetFunctionPointer();

            if (pTargetMethod[0] == 0xE9)
            {
                Int32 rel32 = *(Int32*)(pTargetMethod + 1);
                pTargetMethod += rel32 + 5;
            }

            HijackUnmanagedMethod(pTargetMethod, hook, context);
        }
        public static unsafe void HijackUnmanagedMethod(void* target, MethodInfo hook, HijackContextBase context)
        {
            Byte* pHookMethod = (Byte*)hook.MethodHandle.GetFunctionPointer();
            HijackUnmanagedMethod(target, pHookMethod, context);
        }
        public static unsafe void HijackUnmanagedMethod(void* target, void* hook, HijackContextBase context)
        {
            for (int idx = 0; idx < 64; idx++)
                context.Backup[idx] = ((Byte*)target)[idx];

            // We need to replace at least 13 byte, for `mov rax, [addr]`, `jmp rax`, and `push rax` instruction.
            const int minimumCount = 13;
            MigrationResult result = context.MigrateInstruction((IntPtr)target, (IntPtr)context.Buffer, minimumCount);
            Debug.Assert(result.SrcOffset >= minimumCount);

            context.ReplacedByteCount = result.SrcOffset;

            Kernel32.VirtualProtectEx(
                Process.GetCurrentProcess().Handle, (IntPtr)target, (UIntPtr)result.SrcOffset,
                Kernel32.PageProtection.ExecuteReadWrite, out Kernel32.PageProtection originalProtection);
            
            ((Byte*)context.Buffer)[result.DstOffset] = 0x50; // push rax
            InsertJump(context.Buffer + result.DstOffset + 1, (Byte*)target + result.SrcOffset - 1);

            InsertJump(target, hook);
            ((Byte*)target)[result.SrcOffset - 1] = 0x58; // pop rax

            Kernel32.VirtualProtectEx(
                Process.GetCurrentProcess().Handle, (IntPtr)target, (UIntPtr)result.SrcOffset,
                originalProtection, out _);
        }

        public static unsafe void RestoreManagedMethod(MethodInfo target, HijackContextBase context)
        {
            Byte* pTargetMethod = (Byte*)target.MethodHandle.GetFunctionPointer();

            if (pTargetMethod[0] == 0xE9)
            {
                Int32 rel32 = *(Int32*)(pTargetMethod + 1);
                pTargetMethod += rel32 + 5;
            }

            RestoreUnmanagedMethod(pTargetMethod, context);
        }
        public static unsafe void RestoreUnmanagedMethod(void* target, HijackContextBase context)
        {
            Kernel32.VirtualProtectEx(
                Process.GetCurrentProcess().Handle, (IntPtr)target, (UIntPtr)context.ReplacedByteCount,
                Kernel32.PageProtection.ExecuteReadWrite, out Kernel32.PageProtection originalProtection);

            if (context.IsDisposed)
                throw new ObjectDisposedException("context is already disposed. Is method already restored?");

            for (int idx = 0; idx < context.ReplacedByteCount; idx++)
                ((Byte*)target)[idx] = context.Backup[idx];

            context.Dispose();

            Kernel32.VirtualProtectEx(
                Process.GetCurrentProcess().Handle, (IntPtr)target, (UIntPtr)context.ReplacedByteCount,
                originalProtection, out _);
        }

        // Note:
        // This method does not patch atomically, so need to make sure target method is not under use.
        private static unsafe void InsertJump(void* target, void* jmpLocation)
        {
            Byte* p = (Byte*)target;

            if (sizeof(void*) == sizeof(Int32))
            {
                p[0] = 0xB8;
                p[5] = 0xFF;
                p[6] = 0xE0;

                *(void**)(p + 1) = jmpLocation;
            }
            else if (sizeof(void*) == sizeof(Int64))
            {
                p[0] = 0x48;
                p[1] = 0xB8;
                p[10] = 0xFF;
                p[11] = 0xE0;

                *(void**)(p + 2) = jmpLocation;
            }
            else
            {
                throw new ArgumentException("Unexpected pointer size");
            }
        }
    }
}
