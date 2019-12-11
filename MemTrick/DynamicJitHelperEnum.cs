namespace MemTrick
{
    public enum DynamicJitHelperEnum
    {
        Dbl2Int,
        Dbl2Lng,
        Dbl2Uint,

        NewSFast,
        NewSFastAlign8,

        NewArr1Obj,
        NewArr1VC,
        NewArr1Align8,

        HelpBox,

        ArrAddrSt,

        StopForGC,

        AssignRef,
        CheckedAssignRef,
        AssignByRef,
        
        GetSharedGCStaticBase,
        GetSharedNonGCStaticBase,
        GetSharedGCStaticBaseNoCtor,
        GetSharedNonGCStaticBaseNoCtor,

        ProfFcnEnter,
        ProfFcnLeave,
        ProfFcnTailcall,

        InitPInvokeFrame
    }
}
