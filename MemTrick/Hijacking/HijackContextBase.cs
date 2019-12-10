using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MemTrick.Hijacking
{
    public abstract unsafe class HijackContextBase
    {
        public bool IsDisposed
        {
            get;
            private set;
        }

        private const int BufferSize = 64;
        protected static readonly FieldInfo MethodPtrAuxMethodInfo;

        static HijackContextBase()
        {
            MethodPtrAuxMethodInfo = typeof(Delegate).GetField("_methodPtrAux", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal Byte* Buffer
        {
            get;
            private set;
        }
        internal Byte* Backup
        {
            get;
            private set;
        }
        public int ReplacedByteCount
        {
            get;
            internal set;
        }

#if DEBUG
        private StackTrace st = new StackTrace(true);
#endif

        public delegate MigrationResult MigrationInstructionDelegate(IntPtr src, IntPtr dst, Int32 minimumCount);
        public MigrationInstructionDelegate MigrateInstruction;

        protected HijackContextBase()
        {
            Buffer = (Byte*)Marshal.AllocHGlobal(BufferSize);
            Backup = (Byte*)Marshal.AllocHGlobal(BufferSize);

            RawMemoryAllocator.FillMemory(Buffer, 0xCC, BufferSize);
            RawMemoryAllocator.FillMemory(Backup, 0xCC, BufferSize);

            Kernel32.VirtualProtectEx(
                Process.GetCurrentProcess().Handle, (IntPtr)Buffer, (UIntPtr)BufferSize,
                Kernel32.PageProtection.ExecuteReadWrite, out _);
        }
        ~HijackContextBase()
        {
#if DEBUG
            Environment.FailFast(".dtor of HijackContextBase has invoked!");
#endif

            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            Marshal.FreeHGlobal((IntPtr)Buffer);
            Marshal.FreeHGlobal((IntPtr)Backup);

            Buffer = null;
            Backup = null;
        }
    }
}
