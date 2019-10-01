namespace MemTrick
{
    /// <summary>
    /// Note: After .NET 4.6, this class can be replaced by using Array.Empty`1 method.
    /// </summary>
    internal static class ArrayHolder<T>
    {
        public static T[] Empty = new T[0];
    }
}
