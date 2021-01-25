using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemTrick
{
    internal unsafe sealed class BytePtrCodeReader : CodeReader
    {
        private Byte* p;

        public BytePtrCodeReader(Byte* p)
        {
            this.p = p;
        }
        public BytePtrCodeReader(IntPtr p)
        {
            this.p = (Byte*)p;
        }

        public override int ReadByte()
        {
            return *(p++);
        }
    }
}
