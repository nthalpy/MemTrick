using System;

namespace MemTrick.Test.Exceptions
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