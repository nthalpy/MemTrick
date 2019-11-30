using System;

namespace MemTrick.Hijacking
{
    public unsafe sealed class HijackFuncContext<T1, TResult> : HijackContextBase
    {
        // Dummy method for arbitary code execution
        private static TResult DummyMethod(T1 _1)
        {
            throw new NotImplementedException();
        }
        
        public HijackFuncContext()
            : base()
        {
            Funclet = DummyMethod;
            MethodPtrAuxMethodInfo.SetValue(Funclet, (IntPtr)Buffer);
        }

        public readonly Func<T1, TResult> Funclet;
    }
}
