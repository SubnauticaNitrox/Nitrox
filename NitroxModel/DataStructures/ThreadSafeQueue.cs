using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [DebuggerDisplay("Items = {" + nameof(queue) + "}")]
    [ProtoContract]
    [Serializable]
    public class ThreadSafeQueue<T> : IReadOnlyCollection<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoIgnore]
        private readonly object locker = new();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ProtoMember(1)]
        private readonly Queue<T> queue;

        public int Count
        {
            get
            {
                lock (locker)
                {
                    return queue.Count;
                }
            }
        }

        public ThreadSafeQueue()
        {
            queue = new Queue<T>();
        }

        public ThreadSafeQueue(int initialCapacity)
        {
            queue = new Queue<T>(initialCapacity);
        }

        public ThreadSafeQueue(IEnumerable<T> values)
        {
            queue = new Queue<T>(values);
        }

        public ThreadSafeQueue(Queue<T> queue, bool createCopy = true)
        {
            this.queue = createCopy ? CreateCopy(queue) : queue;
        }
        public void Clear()
        {
            lock (locker)
            {
                queue.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return queue.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                queue.CopyTo(array, arrayIndex);
            }
        }

        public T Dequeue()
        {
            lock (locker)
            {
                return queue.Dequeue();
            }
        }

        public void Enqueue(T item)
        {
            lock (locker)
            {
                queue.Enqueue(item);
            }
        }

        public T Peek()
        {
            lock (locker)
            {
                return queue.Peek();
            }
        }

        public T[] ToArray()
        {
            lock (locker)
            {
                return queue.ToArray();
            }
        }

        public void TrimExcess()
        {
            lock (locker)
            {
                queue.TrimExcess();
            }
        }

        public IEnumerable<T> Clone()
        {
            lock (locker)
            {
                return CreateCopy(queue);
            }
        }

        private Queue<T> CreateCopy(IEnumerable<T> data)
        {
            return new Queue<T>(data);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker)
            {
                return CreateCopy(queue).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
