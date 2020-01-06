using System;

namespace MemTrick.RumtimeSpecific
{
    internal static unsafe class ArbitaryMethodInvoker
    {
        private static UnmanagedHeapDisposeHandle CreateAction<TAction, TArg0>(TArg0 arg0, IntPtr pMethod, out TAction action)
            where TAction : Delegate
            where TArg0 : class
        {
            MethodTable* pMT = MethodTable.GetMethodTable<TAction>();

            ObjectHeader* pThis = TypedReferenceHelper.ClassToPointer(arg0);
            ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(pMT->BaseSize);

            try
            {
                objHeader->SyncBlock = 0;
                objHeader->MethodTable = pMT;

                Byte* objBody = (Byte*)(objHeader + 1);
                *(MethodTable***)objBody = &pThis->MethodTable; // Delegate._target
                *(IntPtr*)(objBody + 2 * sizeof(IntPtr)) = pMethod; // Delegate._methodPtr

                action = TypedReferenceHelper.PointerToObject<TAction>(objHeader);
            }
            catch
            {
                RawMemoryAllocator.Free(objHeader);
                throw;
            }

            return new UnmanagedHeapDisposeHandle(objHeader);
        }

        /// <summary>
        /// Invoke pMethod(arg0), or arg0.pMethod()
        /// </summary>
        public static void InvokeAction<T0>(
            IntPtr pMethod,
            T0 arg0)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action action))
                action.Invoke();
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1), or arg0.pMethod(arg1)
        /// </summary>
        public static void InvokeAction<T0, T1>(
            IntPtr pMethod,
            T0 arg0, T1 arg1)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1> action))
                action.Invoke(arg1);
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1, arg2), or arg0.pMethod(arg1, arg2)
        /// </summary>
        public static void InvokeAction<T0, T1, T2>(
            IntPtr pMethod,
            T0 arg0, T1 arg1, T2 arg2)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1, T2> action))
                action.Invoke(arg1, arg2);
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1, arg2, arg3), or arg0.pMethod(arg1, arg2, arg3)
        /// </summary>
        public static void InvokeAction<T0, T1, T2, T3>(
            IntPtr pMethod,
            T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1, T2, T3> action))
                action.Invoke(arg1, arg2, arg3);
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1, arg2, arg3, arg4), or arg0.pMethod(arg1, arg2, arg3, arg4)
        /// </summary>
        public static void InvokeAction<T0, T1, T2, T3, T4>(
            IntPtr pMethod,
            T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1, T2, T3, T4> action))
                action.Invoke(arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1, arg2, arg3, arg4, arg5), or arg0.pMethod(arg1, arg2, arg3, arg4, arg5)
        /// </summary>
        public static void InvokeAction<T0, T1, T2, T3, T4, T5>(
            IntPtr pMethod,
            T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1, T2, T3, T4, T5> action))
                action.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1, arg2, arg3, arg4, arg5, arg6), or arg0.pMethod(arg1, arg2, arg3, arg4, arg5, arg6)
        /// </summary>
        public static void InvokeAction<T0, T1, T2, T3, T4, T5, T6>(
            IntPtr pMethod,
            T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            where T0 : class
        {
            using (CreateAction(arg0, pMethod, out Action<T1, T2, T3, T4, T5, T6> action))
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }
    }
}
