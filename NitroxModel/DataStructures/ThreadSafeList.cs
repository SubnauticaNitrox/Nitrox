using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay($"Items = {{{nameof(list)}}}")]
    [DataContract]
    [Serializable]
    public class ThreadSafeList<T> : IList<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [IgnoreDataMember]
        private readonly object locker = new();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DataMember(Order = 1)]
        [SerializableMember]
        private List<T> list;

        public T this[int i]
        {
            get
            {
                lock (locker)
                {
                    return list[i];
                }
            }
            set
            {
                lock (locker)
                {
                    list[i] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return list.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public ThreadSafeList()
        {
            list = new List<T>();
        }

        public ThreadSafeList(int initialCapacity)
        {
            list = new List<T>(initialCapacity);
        }

        public ThreadSafeList(IEnumerable<T> values)
        {
            list = new List<T>(values);
        }

        public ThreadSafeList(List<T> list, bool createCopy = true)
        {
            this.list = createCopy ? CreateCopy(list) : list;
        }

        public void Add(T item)
        {
            lock (locker)
            {
                list.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            lock (locker)
            {
                list.AddRange(collection);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                list.Clear();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                return list.Remove(item);
            }
        }

        public int IndexOf(T item)
        {
            lock (locker)
            {
                return list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (locker)
            {
                list.Insert(index, item);
            }
        }

        public bool RemoveAt(int index)
        {
            lock (locker)
            {
                if (index >= list.Count || index < 0)
                {
                    return false;
                }

                list.RemoveAt(index);
                return true;
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return list.Contains(item);
            }
        }

        public bool TryGetValue(int index, out T item)
        {
            lock (locker)
            {
                if (index <= list.Count || index < 0)
                {
                    item = default;
                    return false;
                }
                item = list[index];
                return true;
            }
        }

        public List<T> ToList()
        {
            lock (locker)
            {
                return new List<T>(list);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                return CreateCopy(list).GetEnumerator();
            }
        }

        /// <summary>
        ///     Clears the list and adds the given items.
        /// </summary>
        /// <param name="items">Items to add onto the empty list.</param>
        public void Set(IEnumerable<T> items)
        {
            lock (locker)
            {
                list.Clear();
                foreach (T item in items)
                {
                    list.Add(item);
                }
            }
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            lock (locker)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (predicate(list.ElementAt(i)))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        public IEnumerable<T> Clone()
        {
            lock (locker)
            {
                return CreateCopy(list);
            }
        }

        public T Find(Predicate<T> predicate)
        {
            Validate.NotNull(predicate);

            lock (locker)
            {
                foreach (T item in list)
                {
                    if (predicate(item))
                    {
                        return item;
                    }
                }
                return default;
            }
        }

        void IList<T>.RemoveAt(int index)
        {
            lock (locker)
            {
                list.RemoveAt(index);
            }
        }

        private List<T> CreateCopy(IEnumerable<T> data)
        {
            return new List<T>(data);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
