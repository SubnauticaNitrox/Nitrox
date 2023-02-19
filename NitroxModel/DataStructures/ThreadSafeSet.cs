using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay($"Items = {{{nameof(set)}}}")]
    [DataContract]
    [Serializable]
    public class ThreadSafeSet<T> : ISet<T>
    {
        /// <summary>
        ///     Using a lock object instead of ReaderWriterLockSlim because to overhead of the latter is ~5x and
        ///     we don't run long write operations anywhere in this class.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [IgnoreDataMember]
        private readonly object locker = new();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DataMember(Order = 1)]
        private readonly HashSet<T> set;

        public ThreadSafeSet()
        {
            set = new HashSet<T>();
        }

        public ThreadSafeSet(HashSet<T> set, bool createCopy = true)
        {
            if (set == null || createCopy)
            {
                this.set = CreateCopy(set);
                return;
            }
            this.set = set;
        }

        public ThreadSafeSet(params T[] values)
        {
            set = new HashSet<T>(values);
        }

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return set.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        void ICollection<T>.Add(T item) => Add(item);

        public bool Add(T item)
        {
            lock (locker)
            {
                return set.Add(item);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            lock (locker)
            {
                set.UnionWith(other);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            lock (locker)
            {
                set.IntersectWith(other);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (locker)
            {
                set.ExceptWith(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (locker)
            {
                set.SymmetricExceptWith(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.IsSupersetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.IsProperSupersetOf(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.IsProperSubsetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.Overlaps(other);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (locker)
            {
                return set.SetEquals(other);
            }
        }

        bool ISet<T>.Add(T item)
        {
            lock (locker)
            {
                return set.Add(item);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                set.Clear();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                set.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                return set.Remove(item);
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return set.Contains(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                return CreateCopy(set).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<T> ToList()
        {
            lock (locker)
            {
                return new List<T>(set);
            }
        }

        /// <summary>
        ///     Clears the set and adds the given items.
        /// </summary>
        /// <param name="items">Items to add onto the empty set.</param>
        public void Set(IEnumerable<T> items)
        {
            lock (locker)
            {
                set.Clear();
                foreach (T item in items)
                {
                    set.Add(item);
                }
            }
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            lock (locker)
            {
                set.RemoveWhere(predicate);
            }
        }

        public IEnumerable<T> Clone()
        {
            lock (locker)
            {
                return CreateCopy(set);
            }
        }

        private HashSet<T> CreateCopy(ISet<T> data)
        {
            return new HashSet<T>(data);
        }
    }
}
