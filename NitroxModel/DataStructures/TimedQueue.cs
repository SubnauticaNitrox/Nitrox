using NitroxModel.Packets;
using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures
{
    public class TimedQueue<T> where T : Packet
    {
        struct QueueEntry
        {
            public T item;
            public long insertionTimeInTicks;

            public QueueEntry(T item, long insertionTime)
            {
                this.item = item;
                this.insertionTimeInTicks = insertionTime;
            }
        }

        private Queue<QueueEntry> items;
        private long ticksUntilEligible;

        public TimedQueue(long ticksUntilEligible)
        {
            this.ticksUntilEligible = ticksUntilEligible;
            this.items = new Queue<QueueEntry>();
        }

        public void Enqueue(T item)
        {
            lock (items)
            {
                //may need to DateTime.Now.AddTick to prevent out of order when restoring many packets
                items.Enqueue(new QueueEntry(item, DateTime.Now.Ticks));
            }
        }

        public bool HasReadyItems()
        {
            lock (items)
            {
                if (items.Count == 0)
                {
                    return false;
                }

                QueueEntry item = items.Peek();

                return (DateTime.Now.Ticks - item.insertionTimeInTicks) >= ticksUntilEligible;
            }
        }

        public T Dequeue()
        {
            if (HasReadyItems())
            {
                lock (items)
                {
                    return items.Dequeue().item;
                }
            }            

            return null;
        }
    }
}
