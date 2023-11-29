using System;
using System.Collections;
using System.Collections.Generic;

namespace NitroxModel.DataStructures;

/// <summary>
///     Given a fixed size, fills to capacity and then overwrites earliest item.
/// </summary>
public class CircularBuffer<T> : IList<T>
{
    private readonly List<T> data;
    private readonly int maxSize;

    /// <summary>
    ///     Returns the index last changed. If <see cref="CircularBuffer{T}" /> is empty, returns -1.
    /// </summary>
    public int LastChangedIndex { get; protected set; } = -1;

    /// <summary>
    ///     Gets the item at the index, wrapping around the buffer if out-of-range.
    /// </summary>
    public T this[int index]
    {
        // Proper modulus operator which C# doesn't have. % = remainder operator and doesn't work in reverse for negative numbers.
        get => data[Math.Abs((index % data.Count + data.Count) % data.Count)];
        set => throw new NotSupportedException();
    }

    public int Count => data.Count;
    public bool IsReadOnly => false;

    public CircularBuffer(int maxSize, int initialCapacity = 0)
    {
        if (maxSize < 0) throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be larger than -1");

        this.maxSize = maxSize;
        data = new List<T>(Math.Max(0, Math.Min(initialCapacity, maxSize)));
    }

    public int IndexOf(T item)
    {
        return data.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        data.RemoveAt(index);
    }

    public bool Remove(T item)
    {
        return data.Remove(item);
    }

    public void Add(T item)
    {
        if (maxSize == 0) return;
        if (data.Count < maxSize)
        {
            data.Add(item);
            LastChangedIndex++;
            return;
        }

        LastChangedIndex = (LastChangedIndex + 1) % maxSize;
        data[LastChangedIndex] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (T item in items) Add(item);
    }

    public void AddRange(params T[] items)
    {
        foreach (T item in items) Add(item);
    }

    public void Clear()
    {
        data.Clear();
        LastChangedIndex = -1;
    }

    public bool Contains(T item)
    {
        return data.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        data.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
