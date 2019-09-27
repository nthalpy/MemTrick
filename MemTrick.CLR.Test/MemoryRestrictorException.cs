using System;

namespace MemTrick.CLR.Test
{
    [Serializable]
    internal class MemoryRestrictorException : Exception
    {
        public MemoryRestrictorException(String message)
            : base(message)
        {
        }
    }
}