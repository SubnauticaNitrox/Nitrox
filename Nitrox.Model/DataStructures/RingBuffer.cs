using System;

namespace Nitrox.Model.DataStructures;

/// <summary>
/// A dynamically expanding, circular first-in, first-out array that reduces memory allocations by reusing slots.
/// Automatically doubles its capacity when full.
/// </summary>
public class RingBuffer<T>
{
    private T[] buffer;
    private int capacity;
    public int Head { get; private set; }
    public int Tail => (Head + Count) % capacity;
    public int Count { get; private set; }

    /// <summary>
    /// Gets or sets the element at the wrapped index.
    /// </summary>
    public T this[int index]
    {
        get => buffer[index % capacity];
        private set => buffer[index % capacity] = value;
    }

    public T First => Count > 0 ? buffer[Head] : throw new InvalidOperationException("Buffer is empty");

    public T Last => Count > 0 ? buffer[(Head + Count - 1) % capacity] : throw new InvalidOperationException("Buffer is empty");

    public RingBuffer(int initialCapacity = 64)
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
        int newCapacity = capacity * 2;
        T[] newBuffer = new T[newCapacity];
        for (int i = 0; i < Count; i++)
        {
            newBuffer[i] = buffer[(Head + i) % capacity];
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

        Count--;
        buffer[Tail] = default; // avoid memory leaks
    }
}
