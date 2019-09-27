using System;

namespace MemTrick.CLR.Test.Exceptions
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