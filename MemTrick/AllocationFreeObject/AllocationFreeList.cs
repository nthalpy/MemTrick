using MemTrick.RumtimeSpecific;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MemTrick.AllocationFreeObject
{
    /// <summary>
    /// Allocation-Free version of System.Collections.Generic.List`1.
    /// Sync w/ src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs
    /// @Harnel
    /// </summary>
    //[DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class AllocationFreeList<T> : /*IList<T>, IList, IReadOnlyList<T>,*/ IDisposable
    {
        private static readonly T[] EmptyArray;

        static AllocationFreeList()
        {
            // Note: emptyArray will not be free in any case.
            UnmanagedHeapAllocator.AllocateSZArray<T>(0, out EmptyArray);
        }

        /// <summary>
        /// List which contains methods.
        /// Most of methods does not reallocate _items member, 
        /// therefore we can just focus on methods which reallocate _items.
        /// </summary>
        private readonly List<T> list;
        private readonly UnmanagedHeapDisposeHandle listHandle;

        // Note:
        // arrayHandle is null when array is AllocationFreeList`1.EmptyArray.
        private Nullable<UnmanagedHeapDisposeHandle> arrayHandle;

        // Synced w/ list._items
        private readonly IntPtr ppItems;
        private unsafe T[] items
        {
            get
            {
                IntPtr pItems = *(IntPtr*)ppItems;
                if (pItems == IntPtr.Zero)
                    return null;

                IntPtr ppMT = pItems;
                IntPtr pSyncBlock = ppMT - sizeof(IntPtr);

                return TypedReferenceHelper.PointerToObject<T[]>((ObjectHeader*)pSyncBlock);
            }
            set
            {
                *(IntPtr*)ppItems = (IntPtr)(&TypedReferenceHelper.ClassToPointer(value)->MethodTable);
            }
        }

        private readonly IntPtr pSize;
        private unsafe int size
        {
            get
            {
                return *(int*)pSize;
            }
            set
            {
                *(int*)pSize = value;
            }
        }

        // Synced w/ list._items
        private readonly IntPtr pVersion;
        private unsafe int version
        {
            get
            {
                return *(int*)pVersion;
            }
            set
            {
                *(int*)pVersion = value;
            }
        }

        public AllocationFreeList()
        {
            unsafe
            {
                listHandle = UnmanagedHeapAllocator.UninitializedAllocation(out list);

                ppItems = (IntPtr)((Byte*)listHandle.ObjHeader + sizeof(ObjectHeader));
                pSize = ppItems + sizeof(IntPtr);
                pVersion = pSize + sizeof(Int32);
            }

            items = EmptyArray;
        }
        public unsafe AllocationFreeList(int capacity)
            : this()
        {
            if (capacity == 0)
                items = EmptyArray;
            else
            {
                arrayHandle = UnmanagedHeapAllocator.AllocateSZArray(capacity, out T[] temp);
                items = temp;
            }
        }
        public AllocationFreeList(IEnumerable<T> collection)
        {
            throw new NotImplementedException();
        }

        private void EnsureCapacity(int min)
        {
            if (items.Length < min)
            {
                const int DefaultCapacity = 4;
                const int MaxArrayLength = 0X7FEFFFFF;

                int newCapacity = items.Length == 0 ? DefaultCapacity : items.Length * 2;
                if ((uint)newCapacity > MaxArrayLength)
                    newCapacity = MaxArrayLength;
                if (newCapacity < min)
                    newCapacity = min;

                this.Capacity = newCapacity;
            }
        }

        #region List`1 public interface
        public int Capacity
        {
            get
            {
                return items.Length;
            }
            set
            {
                if (value < list.Count)
                {
                    // Forward to List`1.Capacity so we can throw exception through BCL code.
                    list.Capacity = value;
                }

                if (value != list.Count)
                {
                    if (value > 0)
                    {
                        UnmanagedHeapDisposeHandle newHandle = UnmanagedHeapAllocator.AllocateSZArray(value, out T[] newItems);
                        if (list.Count > 0)
                        {
                            Array.Copy(items, newItems, list.Count);
                            arrayHandle?.Dispose();
                        }

                        items = newItems;
                        arrayHandle = newHandle;
                    }
                    else
                    {
                        arrayHandle?.Dispose();
                        arrayHandle = null;

                        items = EmptyArray;
                    }
                }
            }
        }
        public int Count
        {
            get
            {
                return list.Count;
            }
        }
        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public void Add(T item)
        {
            EnsureCapacity(list.Count + 1);
            list.Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> col)
            {
                EnsureCapacity(list.Count + col.Count);
                list.AddRange(collection);
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                        Add(en.Current);
                }

                version++;
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            throw new NotImplementedException();
        }

        public int BinarySearch(T item)
        {
            return list.BinarySearch(item);
        }
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return list.BinarySearch(item, comparer);
        }
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return list.BinarySearch(index, count, item, comparer);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array)
        {
            list.CopyTo(array);
        }
        public void CoypTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            list.CopyTo(index, array, arrayIndex, count);
        }

        public bool Exists(Predicate<T> match)
        {
            return list.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return list.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            throw new NotImplementedException();
        }

        public int FindIndex(Predicate<T> match)
        {
            return list.FindIndex(match);
        }
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return list.FindIndex(startIndex, match);
        }
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public List<T> GetRange(int index, int count)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }
        public int IndexOf(T item, int index)
        {
            return list.IndexOf(item, index);
        }
        public int IndexOf(T item, int index, int count)
        {
            return list.IndexOf(item, index, count);
        }

        public void Insert(int index, T item)
        {
            if (list.Count == Capacity)
                EnsureCapacity(list.Count + 1);

            list.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                EnsureCapacity(list.Count + c.Count);
                list.InsertRange(index, collection);
            }
            else
            {
                foreach (T item in collection)
                    Add(item);

                version++;
            }
        }

        public int LastIndexOf(T item)
        {
            return list.LastIndexOf(item);
        }
        public int LastIndexOf(T item, int index)
        {
            return list.LastIndexOf(item, index);
        }
        public int LastIndexOf(T item, int index, int count)
        {
            return list.LastIndexOf(item, index, count);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public int RemoveAll(Predicate<T> predicate)
        {
            return list.RemoveAll(predicate);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            list.RemoveRange(index, count);
        }

        public void Reverse()
        {
            list.Reverse();
        }
        public void Reverse(int index, int count)
        {
            list.Reverse(index, count);
        }

        public void Sort()
        {
            list.Sort();
        }
        public void Sort(IComparer<T> comparer)
        {
            list.Sort(comparer);
        }
        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            list.Sort(index, count, comparer);
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }
        
        public void TrimExcess()
        {
            int threshold = (int)(((double)Capacity) * 0.9);
            if (list.Count < threshold)
            {
                Capacity = list.Count;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return list.TrueForAll(match);
        }
        #endregion
        #region Explicit interface implementation
        #endregion

        public void Dispose()
        {
            listHandle.Dispose();
            arrayHandle?.Dispose();
        }
    }
}
