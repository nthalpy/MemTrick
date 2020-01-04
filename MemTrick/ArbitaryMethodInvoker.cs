using MemTrick.RumtimeSpecific;
using System;

namespace MemTrick
{
    internal static class ArbitaryMethodInvoker
    {
        /// <summary>
        /// Invoke pMethod(arg0), or arg0.pMethod()
        /// </summary>
        public static void InvokeAction<T0>(IntPtr pMethod, T0 arg0) where T0 : class
        {
            unsafe
            {
                MethodTable* pMT = MethodTable.GetMethodTable<Action>();

                ObjectHeader* pThis = TypedReferenceHelper.ClassToPointer(arg0);
                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(pMT->BaseSize);

                try
                {
                    objHeader->SyncBlock = 0;
                    objHeader->MethodTable = pMT;

                    Byte* objBody = (Byte*)(objHeader + 1);
                    *(MethodTable***)objBody = &pThis->MethodTable; // Delegate._target
                    *(IntPtr*)(objBody + 2 * sizeof(IntPtr)) = pMethod; // Delegate._methodPtr

                    Action action = TypedReferenceHelper.PointerToObject<Action>(objHeader);
                    action.Invoke();
                }
                finally
                {
                    RawMemoryAllocator.Free(objHeader);
                }
            }
        }

        /// <summary>
        /// Invoke pMethod(arg0, arg1), or arg0.pMethod(arg1)
        /// </summary>
        public static void InvokeAction<T0, T1>(IntPtr pMethod, T0 arg0, T1 arg1) where T0 : class
        {
            unsafe
            {
                MethodTable* pMT = MethodTable.GetMethodTable<Action<T1>>();

                ObjectHeader* pThis = TypedReferenceHelper.ClassToPointer(arg0);
                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(pMT->BaseSize);

                try
                {
                    objHeader->SyncBlock = 0;
                    objHeader->MethodTable = pMT;

                    Byte* objBody = (Byte*)(objHeader + 1);
                    *(MethodTable***)objBody = &pThis->MethodTable; // Delegate._target
                    *(IntPtr*)(objBody + 2 * sizeof(IntPtr)) = pMethod; // Delegate._methodPtr

                    Action<T1> action = TypedReferenceHelper.PointerToObject<Action<T1>>(objHeader);
                    action.Invoke(arg1);
                }
                finally
                {
                    RawMemoryAllocator.Free(objHeader);
                }
            }
        }
    }
}
