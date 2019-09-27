using System;

namespace MemTrick.CLR
{
    /// <summary>
    /// Pattern for assembly.
    /// </summary>
    internal sealed class AssemblyPattern
    {
        #region static fields
        public readonly static AssemblyPattern CLR_JIT_TrialAllocSFastMP_InlineGetThread = new AssemblyPattern(
            // Assembly dumped from WinDbg, with runtime .NET Framework 4.7.2 is following, 
            // but maybe some constants can be different by runtimes.
            //
            // clr!JIT_TrialAllocSFastMP_InlineGetThread:
            //     8b5104             mov edx,dword ptr[rcx + 4]
            // clr!JIT_TrialAllocSFastMP_InlineGetThread__PatchTLSOffset:
            //     654c8b1c2598140000 mov r11,qword ptr gs:[1498h]
            //     4d8b5370           mov r10,qword ptr[r11 + 70h]
            //     498b4368           mov rax,qword ptr[r11 + 68h]
            //     4803d0             add rdx, rax
            //     493bd2             cmp rdx,r10
            //     7708               ja clr!JIT_TrialAllocSFastMP_InlineGetThread__PatchTLSOffset+0x21
            //     49895368           mov qword ptr[r11 + 68h],rdx
            //     488908             mov qword ptr[rax],rcx
            //     c3                 ret
            //     e937b20000         jmp clr!JIT_New
            new Nullable<Byte>[]
            {
                0x8B, 0x51, null,
                0x65, 0x4c, 0x8B, 0x1C, 0x25, null, null, null, null,
                0x4D, 0x8B, 0x53, null,
                0x49, 0x8B, 0x43, null,
                0x48, 0x03, 0xD0,
                0x49, 0x3B, 0xD2,
                0x77, 0x08,
                0x49, 0x89, 0x53, null,
                0x48, 0x89, 0x08,
                0xC3,
                0xE9, null, null, null, null,
            },
            new int[]
            {
                0x25, // 0x25: jmp clr!JIT_New
            });
        #endregion

        /// <summary>
        /// Pattern of assembly. null in array is equal to asterisk.
        /// </summary>
        private readonly Nullable<Byte>[] pattern;

        /// <summary>
        /// Offset of rel32 address which locates somewhere out method, like JIT_New, __tls_index, ...
        /// </summary>
        public readonly int[] NonLocalRel32Offset;
        
        public int Size
        {
            get
            {
                return pattern.Length;
            }
        }

        private AssemblyPattern(Nullable<Byte>[] pattern, int[] nonLocalRel32Offset)
        {
            this.pattern = pattern;
            NonLocalRel32Offset = nonLocalRel32Offset;
        }

        public unsafe bool IsMatches(void* ptr)
        {
            Byte* bytePtr = (Byte*)ptr;
            for (int idx = 0; idx < pattern.Length; idx++)
                if (pattern[idx].HasValue && bytePtr[idx] != pattern[idx].Value)
                    return false;

            return true;
        }
    }
}
