using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NitroxModel.DataStructures
{
    public class PriorityQueue<T>
    {
        public PriorityQueue()
        {
            // Build the collection of priority chains.
            priorityChains = new SortedList<int, PriorityChain<T>>(); // NOTE: should be Priority
            cacheReusableChains = new Stack<PriorityChain<T>>(10);

            head = tail = null;
            count = 0;
        }

        // NOTE: not used
        // public int Count {get{return _count;}}

        public int MaxPriority // NOTE: should be Priority
        {
            get
            {
                int count = priorityChains.Count;

                if (count > 0)
                {
                    return priorityChains.Keys[count - 1];
                }
                else
                {
                    return int.MaxValue; // NOTE: should be Priority.Invalid;
                }
            }
        }

        public int Count => priorityChains.Count;

        public PriorityItem<T> Enqueue(int priority, T data) // NOTE: should be Priority
        {
            // Find the existing chain for this priority, or create a new one
            // if one does not exist.
            PriorityChain<T> chain = GetChain(priority);

            // Wrap the item in a PriorityItem so we can put it in our
            // linked list.
            PriorityItem<T> priorityItem = new PriorityItem<T>(data);

            // Step 1: Append this to the end of the "sequential" linked list.
            InsertItemInSequentialChain(priorityItem, tail);

            // Step 2: Append the item into the priority chain.
            InsertItemInPriorityChain(priorityItem, chain, chain.Tail);

            return priorityItem;
        }

        public T Dequeue()
        {
            // Get the max-priority chain.
            int count = priorityChains.Count;
            if (count > 0)
            {
                PriorityChain<T> chain = priorityChains.Values[count - 1];
                Debug.Assert(chain != null, "PriorityQueue.Dequeue: a chain should exist.");

                PriorityItem<T> item = chain.Head;
                Debug.Assert(item != null, "PriorityQueue.Dequeue: a priority item should exist.");

                RemoveItem(item);

                return item.Data;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public T Peek()
        {
            T data = default(T);

            // Get the max-priority chain.
            int count = priorityChains.Count;
            if (count > 0)
            {
                PriorityChain<T> chain = priorityChains.Values[count - 1];
                Debug.Assert(chain != null, "PriorityQueue.Peek: a chain should exist.");

                PriorityItem<T> item = chain.Head;
                Debug.Assert(item != null, "PriorityQueue.Peek: a priority item should exist.");

                data = item.Data;
            }

            return data;
        }

        public void RemoveItem(PriorityItem<T> item)
        {
            Debug.Assert(item != null, "PriorityQueue.RemoveItem: invalid item.");
            Debug.Assert(item.Chain != null, "PriorityQueue.RemoveItem: a chain should exist.");

            PriorityChain<T> chain = item.Chain;

            // Step 1: Remove the item from its priority chain.
            RemoveItemFromPriorityChain(item);

            // Step 2: Remove the item from the sequential chain.
            RemoveItemFromSequentialChain(item);

            // Note: we do not clean up empty chains on purpose to reduce churn.
        }

        public void ChangeItemPriority(PriorityItem<T> item, int priority) // NOTE: should be Priority
        {
            // Remove the item from its current priority and insert it into
            // the new priority chain.  Note that this does not change the
            // sequential ordering.

            // Step 1: Remove the item from the priority chain.
            RemoveItemFromPriorityChain(item);

            // Step 2: Insert the item into the new priority chain.
            // Find the existing chain for this priority, or create a new one
            // if one does not exist.
            PriorityChain<T> chain = GetChain(priority);
            InsertItemInPriorityChain(item, chain);
        }

        private PriorityChain<T> GetChain(int priority) // NOTE: should be Priority
        {
            PriorityChain<T> chain = null;

            int count = priorityChains.Count;
            if (count > 0)
            {
                if (priority == (int)priorityChains.Keys[0])
                {
                    chain = priorityChains.Values[0];
                }
                else if (priority == (int)priorityChains.Keys[count - 1])
                {
                    chain = priorityChains.Values[count - 1];
                }
                else if ((priority > (int)priorityChains.Keys[0]) &&
                        (priority < (int)priorityChains.Keys[count - 1]))
                {
                    priorityChains.TryGetValue((int)priority, out chain);
                }
            }

            if (chain == null)
            {
                if (cacheReusableChains.Count > 0)
                {
                    chain = cacheReusableChains.Pop();
                    chain.Priority = priority;
                }
                else
                {
                    chain = new PriorityChain<T>(priority);
                }

                priorityChains.Add((int)priority, chain);
            }

            return chain;
        }

        private void InsertItemInPriorityChain(PriorityItem<T> item, PriorityChain<T> chain)
        {
            // Scan along the sequential chain, in the previous direction,
            // looking for an item that is already in the new chain.  We will
            // insert ourselves after the item we found.  We can short-circuit
            // this search if the new chain is empty.
            if (chain.Head == null)
            {
                Debug.Assert(chain.Tail == null, "PriorityQueue.InsertItemInPriorityChain: both the head and the tail should be null.");
                InsertItemInPriorityChain(item, chain, null);
            }
            else
            {
                Debug.Assert(chain.Tail != null, "PriorityQueue.InsertItemInPriorityChain: both the head and the tail should not be null.");

                PriorityItem<T> after = null;

                // Search backwards along the sequential chain looking for an
                // item already in this list.
                for (after = item.SequentialPrev; after != null; after = after.SequentialPrev)
                {
                    if (after.Chain == chain)
                    {
                        break;
                    }
                }

                InsertItemInPriorityChain(item, chain, after);
            }
        }

        internal void InsertItemInPriorityChain(PriorityItem<T> item, PriorityChain<T> chain, PriorityItem<T> after)
        {
            Debug.Assert(chain != null, "PriorityQueue.InsertItemInPriorityChain: a chain must be provided.");
            Debug.Assert(item.Chain == null && item.PriorityPrev == null && item.PriorityNext == null, "PriorityQueue.InsertItemInPriorityChain: item must not already be in a priority chain.");

            item.Chain = chain;

            if (after == null)
            {
                // Note: passing null for after means insert at the head.

                if (chain.Head != null)
                {
                    Debug.Assert(chain.Tail != null, "PriorityQueue.InsertItemInPriorityChain: both the head and the tail should not be null.");

                    chain.Head.PriorityPrev = item;
                    item.PriorityNext = chain.Head;
                    chain.Head = item;
                }
                else
                {
                    Debug.Assert(chain.Tail == null, "PriorityQueue.InsertItemInPriorityChain: both the head and the tail should be null.");

                    chain.Head = chain.Tail = item;
                }
            }
            else
            {
                item.PriorityPrev = after;

                if (after.PriorityNext != null)
                {
                    item.PriorityNext = after.PriorityNext;
                    after.PriorityNext.PriorityPrev = item;
                    after.PriorityNext = item;
                }
                else
                {
                    Debug.Assert(item.Chain.Tail == after, "PriorityQueue.InsertItemInPriorityChain: the chain's tail should be the item we are inserting after.");
                    after.PriorityNext = item;
                    chain.Tail = item;
                }
            }

            chain.Count++;
        }

        private void RemoveItemFromPriorityChain(PriorityItem<T> item)
        {
            Debug.Assert(item != null, "PriorityQueue.RemoveItemFromPriorityChain: invalid item.");
            Debug.Assert(item.Chain != null, "PriorityQueue.RemoveItemFromPriorityChain: a chain should exist.");

            // Step 1: Fix up the previous link
            if (item.PriorityPrev != null)
            {
                Debug.Assert(item.Chain.Head != item, "PriorityQueue.RemoveItemFromPriorityChain: the head should not point to this item.");

                item.PriorityPrev.PriorityNext = item.PriorityNext;
            }
            else
            {
                Debug.Assert(item.Chain.Head == item, "PriorityQueue.RemoveItemFromPriorityChain: the head should point to this item.");

                item.Chain.Head = item.PriorityNext;
            }

            // Step 2: Fix up the next link
            if (item.PriorityNext != null)
            {
                Debug.Assert(item.Chain.Tail != item, "PriorityQueue.RemoveItemFromPriorityChain: the tail should not point to this item.");

                item.PriorityNext.PriorityPrev = item.PriorityPrev;
            }
            else
            {
                Debug.Assert(item.Chain.Tail == item, "PriorityQueue.RemoveItemFromPriorityChain: the tail should point to this item.");

                item.Chain.Tail = item.PriorityPrev;
            }

            // Step 3: cleanup
            item.PriorityPrev = item.PriorityNext = null;
            item.Chain.Count--;
            if (item.Chain.Count == 0)
            {
                if (item.Chain.Priority == (int)priorityChains.Keys[priorityChains.Count - 1])
                {
                    priorityChains.RemoveAt(priorityChains.Count - 1);
                }
                else
                {
                    priorityChains.Remove((int)item.Chain.Priority);
                }

                if (cacheReusableChains.Count < 10)
                {
                    item.Chain.Priority = int.MaxValue; // NOTE: should be Priority.Invalid
                    cacheReusableChains.Push(item.Chain);
                }
            }

            item.Chain = null;
        }

        internal void InsertItemInSequentialChain(PriorityItem<T> item, PriorityItem<T> after)
        {
            Debug.Assert(item.SequentialPrev == null && item.SequentialNext == null, "PriorityQueue.InsertItemInSequentialChain: item must not already be in the sequential chain.");

            if (after == null)
            {
                // Note: passing null for after means insert at the head.

                if (head != null)
                {
                    Debug.Assert(tail != null, "PriorityQueue.InsertItemInSequentialChain: both the head and the tail should not be null.");

                    head.SequentialPrev = item;
                    item.SequentialNext = head;
                    head = item;
                }
                else
                {
                    Debug.Assert(tail == null, "PriorityQueue.InsertItemInSequentialChain: both the head and the tail should be null.");

                    head = tail = item;
                }
            }
            else
            {
                item.SequentialPrev = after;

                if (after.SequentialNext != null)
                {
                    item.SequentialNext = after.SequentialNext;
                    after.SequentialNext.SequentialPrev = item;
                    after.SequentialNext = item;
                }
                else
                {
                    Debug.Assert(tail == after, "PriorityQueue.InsertItemInSequentialChain: the tail should be the item we are inserting after.");
                    after.SequentialNext = item;
                    tail = item;
                }
            }

            count++;
        }

        private void RemoveItemFromSequentialChain(PriorityItem<T> item)
        {
            Debug.Assert(item != null, "PriorityQueue.RemoveItemFromSequentialChain: invalid item.");

            // Step 1: Fix up the previous link
            if (item.SequentialPrev != null)
            {
                Debug.Assert(head != item, "PriorityQueue.RemoveItemFromSequentialChain: the head should not point to this item.");

                item.SequentialPrev.SequentialNext = item.SequentialNext;
            }
            else
            {
                Debug.Assert(head == item, "PriorityQueue.RemoveItemFromSequentialChain: the head should point to this item.");

                head = item.SequentialNext;
            }

            // Step 2: Fix up the next link
            if (item.SequentialNext != null)
            {
                Debug.Assert(tail != item, "PriorityQueue.RemoveItemFromSequentialChain: the tail should not point to this item.");

                item.SequentialNext.SequentialPrev = item.SequentialPrev;
            }
            else
            {
                Debug.Assert(tail == item, "PriorityQueue.RemoveItemFromSequentialChain: the tail should point to this item.");

                tail = item.SequentialPrev;
            }

            // Step 3: cleanup
            item.SequentialPrev = item.SequentialNext = null;
            count--;
        }

        // Priority chains...
        private SortedList<int, PriorityChain<T>> priorityChains; // NOTE: should be Priority
        private Stack<PriorityChain<T>> cacheReusableChains;

        // Sequential chain...
        private PriorityItem<T> head;
        private PriorityItem<T> tail;
        private int count;
    }

    public class PriorityChain<T>
    {
        public PriorityChain(int priority) // NOTE: should be Priority 
        {
            Priority = priority;
        }

        public int Priority { get; set; } // NOTE: should be Priority
        public int Count { get; set; }
        public PriorityItem<T> Head { get; set; }
        public PriorityItem<T> Tail { get; set; }
    }

    public class PriorityItem<T>
    {
        public PriorityItem(T data)
        {
            Data = data;
        }

        public T Data { get; }
        public bool IsQueued => Chain != null;

        // Note: not used
        // public int Priority { get { return _chain.Priority; } } // NOTE: should be Priority 

        internal PriorityItem<T> SequentialPrev { get; set; }
        internal PriorityItem<T> SequentialNext { get; set; }

        internal PriorityChain<T> Chain { get; set; }
        internal PriorityItem<T> PriorityPrev { get; set; }
        internal PriorityItem<T> PriorityNext { get; set; }
    }
}
