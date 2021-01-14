using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MemTrick.AllocationFreeObject
{
    /// <summary>
    /// Identitical class w/ System.Collection.Generic.ICollectionDebugView of dotnet runtime.
    /// Copied because it was internal class.
    /// </summary>
    internal sealed class ICollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public ICollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
