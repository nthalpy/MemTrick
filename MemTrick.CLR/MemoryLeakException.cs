using System;

namespace MemTrick.CLR
{
    public sealed class MemoryLeakException : Exception
    {
        internal MemoryLeakException(String message) 
            : base(message)
        {
        }
    }
}
