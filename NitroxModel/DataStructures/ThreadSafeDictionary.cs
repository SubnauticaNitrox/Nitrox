using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay("Items = {" + nameof(Entries) + "}")]
    [ProtoContract]
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoIgnore]
        private readonly IDictionary<TKey, TValue> dictionary;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoIgnore]
        private readonly object locker = new object();

        [ProtoIgnore]
        public ICollection<TKey> Keys
        {
            get
            {
                lock (locker)
                {
                    return new List<TKey>(dictionary.Keys);
                }
            }
        }

        [ProtoIgnore]
        public ICollection<TValue> Values
        {
            get
            {
                lock (locker)
                {
                    return new List<TValue>(dictionary.Values);
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoIgnore]
        public ICollection<KeyValuePair<TKey, TValue>> Entries
        {
            get
            {
                lock (locker)
                {
                    return dictionary.ToList();
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (locker)
                {
                    return dictionary[key];
                }
            }
            set
            {
                lock (locker)
                {
                    dictionary[key] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return dictionary.Count;
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
            lock (locker)
            {
                dictionary.Add(item);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (locker)
            {
                return dictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (locker)
            {
                dictionary.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (locker)
            {
                return dictionary.Remove(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (locker)
            {
                return dictionary.ContainsKey(key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (locker)
            {
                dictionary.Add(key, value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (locker)
            {
                return dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (locker)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (locker)
            {
                return new Dictionary<TKey, TValue>(dictionary).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
