using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay("Items = {" + nameof(collection) + "}")]
    [ProtoContract]
    [Serializable]
    public class ThreadSafeCollection<T> : IList<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoIgnore]
        private readonly object locker = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoMember(1)]
        private ICollection<T> collection;

        public T this[int i]
        {
            get
            {
                lock (locker)
                {
                    return collection.ElementAt(i);
                }
            }
            set
            {
                lock (locker)
                {
                    IList<T> asList = collection as IList<T>;
                    if (asList != null)
                    {
                        asList[i] = value;
                        return;
                    }

                    ICollection<T> set = CreateCopy(collection);
                    int currentIndex = 0;
                    foreach (T item in collection)
                    {
                        set.Add(i == currentIndex ? value : item);
                        currentIndex++;
                    }
                    collection = set;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return collection.Count;
                }
            }
        }

        public bool IsReadOnly { get; } = false;

        public ThreadSafeCollection()
        {
            collection = new List<T>();
        }

        public ThreadSafeCollection(int initialCapacity)
        {
            collection = new List<T>(initialCapacity);
        }

        public bool IsSet
        {
            get
            {
                lock (locker)
                {
                    return collection is ISet<T>;
                }
            }
        }

        public ThreadSafeCollection(IEnumerable<T> collection, bool createCopy = true)
        {
            ICollection<T> coll = collection as ICollection<T>;
            if (coll == null || createCopy)
            {
                this.collection = CreateCopy(collection);
                return;
            }
            this.collection = coll;
        }

        public void Add(T item)
        {
            lock (locker)
            {
                collection.Add(item);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                collection.Clear();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                collection.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                return collection.Remove(item);
            }
        }

        public int IndexOf(T item)
        {
            lock (locker)
            {
                IList<T> list = collection as IList<T>;
                if (list != null)
                {
                    return list.IndexOf(item);
                }

                int index = 0;
                foreach (T itemIn in collection)
                {
                    if (Equals(itemIn, item))
                    {
                        return index;
                    }
                    index++;
                }
                return -1;
            }
        }

        public void Insert(int index, T item)
        {
            lock (locker)
            {
                IList<T> list = collection as IList<T>;
                if (list != null)
                {
                    list.Insert(index, item);
                    return;
                }

                ICollection<T> newSet = new HashSet<T>();
                int currentIndex = 0;
                foreach (T currentItem in collection)
                {
                    // Add before if at insert index.
                    if (currentIndex == index)
                    {
                        newSet.Add(item);
                    }
                    newSet.Add(currentItem);
                    currentIndex++;
                }
                collection = newSet;
            }
        }

        public bool RemoveAt(int index)
        {
            lock (locker)
            {
                IList<T> list = collection as IList<T>;
                if (list != null)
                {
                    list.RemoveAt(index);
                    return true;
                }
                ISet<T> set = collection as ISet<T>;
                if (set != null)
                {
                    T elem = set.ElementAtOrDefault(index);
                    return elem != null && set.Remove(elem);
                }
                return false;
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return collection.Contains(item);
            }
        }

        public T TryGetValue(int index, out bool succeeded)
        {
            lock (locker)
            {
                succeeded = false;
                T result = collection.ElementAtOrDefault(index);
                succeeded = !Equals(result, default(T));
                return result;
            }
        }

        public List<T> ToList()
        {
            lock (locker)
            {
                return new List<T>(collection);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                return CreateCopy(collection).GetEnumerator();
            }
        }

        /// <summary>
        ///     Clears the collection and adds the given items.
        /// </summary>
        /// <param name="items">Items to add onto the empty collection.</param>
        public void Set(IEnumerable<T> items)
        {
            lock (locker)
            {
                collection.Clear();
                foreach (T item in items)
                {
                    collection.Add(item);
                }
            }
        }

        public void RemoveAll(Func<T, bool> predicate)
        {
            lock (locker)
            {
                foreach (T item in collection)
                {
                    if (predicate(item))
                    {
                        collection.Remove(item);
                    }
                }
            }
        }

        public IEnumerable<T> Clone()
        {
            lock (locker)
            {
                return CreateCopy(collection);
            }
        }

        public T Find(Func<T, bool> predicate)
        {
            Validate.NotNull(predicate);
            
            lock (locker)
            {
                foreach (T item in collection)
                {
                    if (predicate(item))
                    {
                        return item;
                    }
                }
                return default(T);
            }
        }

        void IList<T>.RemoveAt(int index)
        {
            lock (locker)
            {
                IList<T> asList = collection as IList<T>;
                if (asList != null)
                {
                    asList.RemoveAt(index);
                    return;
                }

                ICollection<T> set = CreateCopy(collection);
                int currentIndex = 0;
                foreach (T item in collection)
                {
                    if (index != currentIndex)
                    {
                        set.Add(item);
                    }
                    currentIndex++;
                }
                collection = set;
            }
        }

        private ICollection<T> CreateCopy(IEnumerable<T> data)
        {
            if (data is ISet<T>)
            {
                return new HashSet<T>(data);
            }
            return new List<T>(data);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
