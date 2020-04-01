using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay("Items = {" + nameof(collection) + "}")]
    [ProtoContract]
    [Serializable]
    public class ThreadSafeCollection<T> : IDisposable, IEnumerable<T>
    {
        [ProtoMember(1)]
        private readonly ICollection<T> collection;

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public T this[int i]
        {
            get
            {
                try
                {
                    locker.EnterReadLock();
                    return collection.ElementAt(i);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public int Count
        {
            get
            {
                try
                {
                    locker.EnterReadLock();
                    return collection.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public ThreadSafeCollection()
        {
            collection = new List<T>();
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
            try
            {
                locker.EnterWriteLock();
                collection.Add(item);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                locker.EnterWriteLock();
                return collection.Remove(item);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool RemoveAt(int index)
        {
            try
            {
                locker.EnterWriteLock();
                IList<T> list = collection as IList<T>;
                if (list != null)
                {
                    list.RemoveAt(0);
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
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            try
            {
                locker.EnterReadLock();
                return collection.Contains(item);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public T TryGetValue(int index, out bool succeeded)
        {
            try
            {
                succeeded = false;
                locker.EnterReadLock();
                T result = collection.ElementAtOrDefault(index);
                succeeded = !Equals(result, default(T));
                return result;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Dispose()
        {
            locker?.Dispose();
        }

        public List<T> ToList()
        {
            try
            {
                locker.EnterReadLock();
                return new List<T>(collection);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                locker.EnterReadLock();
                return CreateCopy(collection).GetEnumerator();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        /// <summary>
        ///     Clears the collection and adds the given items.
        /// </summary>
        /// <param name="items">Items to add onto the empty collection.</param>
        public void Set(IEnumerable<T> items)
        {
            try
            {
                locker.EnterWriteLock();
                collection.Clear();
                foreach (T item in items)
                {
                    collection.Add(item);
                }
            }
            finally
            {
                locker.ExitWriteLock();
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
