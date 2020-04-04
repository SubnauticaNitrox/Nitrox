using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [ProtoContract]
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        [ProtoIgnore]
        private readonly IDictionary<TKey, TValue> dictionary;

        [ProtoIgnore]
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        
        public ICollection<TKey> Keys
        {
            get
            {
                try
                {
                    locker.EnterReadLock();
                    return new List<TKey>(dictionary.Keys);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                try
                {
                    locker.EnterReadLock();
                    return new List<TValue>(dictionary.Values);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    locker.EnterReadLock();
                    return dictionary[key];
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    locker.EnterWriteLock();
                    dictionary[key] = value;
                }
                finally
                {
                    locker.ExitWriteLock();
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
                    return dictionary.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly { get; } = false;

        public ThreadSafeDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(IDictionary<TKey, TValue> dictionary, bool createCopy = true)
        {
            this.dictionary = createCopy ? new Dictionary<TKey, TValue>(dictionary) : dictionary;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                locker.EnterWriteLock();
                dictionary.Add(item);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                locker.EnterWriteLock();
                dictionary.Clear();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                locker.EnterReadLock();
                return dictionary.Contains(item);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            try
            {
                locker.EnterReadLock();
                dictionary.CopyTo(array, arrayIndex);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                locker.EnterWriteLock();
                return dictionary.Remove(item);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            try
            {
                locker.EnterReadLock();
                return dictionary.ContainsKey(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Add(TKey key, TValue value)
        {
            try
            {
                locker.EnterWriteLock();
                dictionary.Add(key, value);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool Remove(TKey key)
        {
            try
            {
                locker.EnterWriteLock();
                return dictionary.Remove(key);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                locker.EnterReadLock();
                return dictionary.TryGetValue(key, out value);
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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            try
            {
                locker.EnterReadLock();
                return new Dictionary<TKey, TValue>(dictionary).GetEnumerator();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
