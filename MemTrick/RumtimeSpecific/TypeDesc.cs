using System;
using System.Runtime.InteropServices;

namespace MemTrick.RumtimeSpecific
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TypeDesc
    {
        public Int32 TypeAndFlags;

        public MethodTable* GetMethodTable()
        {
            if (IsGenericVariable())
                return null;

            if (GetInternalCorElementType() == CorElementType.FnPtr)
                return MethodTable.GetMethodTable<UIntPtr>();

            fixed (TypeDesc* pThis = &this)
            {
                ParamTypeDesc* pParam = (ParamTypeDesc*)pThis;

                if (GetInternalCorElementType() == CorElementType.ValueType)
                    return pParam->Arg.AsMethodTable();
                else
                    return pParam->GetTemplateMethodTableInternal();
            }
        }

        private CorElementType GetInternalCorElementType()
        {
            return (CorElementType)(TypeAndFlags & 0xFF);
        }

        private bool IsGenericVariable()
        {
            switch (GetInternalCorElementType())
            {
                case CorElementType.Var:
                case CorElementType.MVar:
                    return true;

                default:
                    return false;
            }
        }
    }
}
