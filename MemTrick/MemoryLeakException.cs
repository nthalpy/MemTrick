using System;

namespace MemTrick
{
    public sealed class MemoryLeakException : Exception
    {
        internal MemoryLeakException(String message) 
            : base(message)
        {
        }
    }
}
