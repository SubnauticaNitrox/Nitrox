using System;
using System.Collections;
using System.Collections.Generic;

namespace Nitrox.Model.DataStructures;

/// <summary>
/// A dynamically expanding, circular first-in, first-out array that reduces memory allocations by reusing slots.
/// Automatically doubles its capacity when full.
/// </summary>
public class ExpandableRingBuffer<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    private T[] buffer;
    private int capacity;
    private int Head { get; set; }
    private int Tail => (Head + Count) % capacity;
    public int Count { get; private set; }

    /// <summary>
    /// Gets or sets the element at the index of the buffer (0 is first element).
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return buffer[(Head + index) % capacity];
        }
    }

    public T First => Count > 0 ? buffer[Head] : throw new InvalidOperationException("Buffer is empty");

    public T Last => Count > 0 ? buffer[(Head + Count - 1) % capacity] : throw new InvalidOperationException("Buffer is empty");

    public ExpandableRingBuffer(int initialCapacity = 64)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), initialCapacity, "Initial capacity must be greater than zero.");
        }

        capacity = initialCapacity;
        buffer = new T[capacity];
    }

    public void Expand()
    {
        int newCapacity = checked(capacity * 2);
        T[] newBuffer = new T[newCapacity];

        // example: [val, val, default, default, default, val, val, val]
        //          left chunk                             right chunk
        // NB: Head is at the beginning of the right chunk, Tail is at the end of the left chunk

        // elements from Head to the end of the array
        int rightChunkLength = Math.Min(Count, capacity - Head);

        Array.Copy(buffer, Head, newBuffer, 0, rightChunkLength);

        // in case there are elements from the beginning of the array to some extent
        int leftChunkLength = Count - rightChunkLength;
        if (leftChunkLength > 0)
        {
            Array.Copy(buffer, 0, newBuffer, rightChunkLength, leftChunkLength);
        }

        capacity = newCapacity;
        buffer = newBuffer;
        Head = 0;
    }

    public void Clear()
    {
        Head = 0;
        Count = 0;
        Array.Clear(buffer, 0, buffer.Length);
    }

    public void Add(T item)
    {
        if (Count == capacity)
        {
            Expand();
        }

        buffer[Tail] = item;
        Count++;
    }

    public void RemoveFirst()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Can't remove an element from an empty ring buffer");
        }

        buffer[Head] = default; // avoid memory leaks
        Head = (Head + 1) % capacity;
        Count--;
    }

    public void RemoveLast()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Can't remove an element from an empty ring buffer");
        }

        buffer[(Head + Count - 1) % capacity] = default; // avoid memory leaks
        Count--;
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return buffer[(Head + i) % capacity];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
