using System;
using System.Collections;
using System.Collections.Generic;

namespace Nitrox.Model.Helper;

public struct StructEnumerator<T>(T[] pool, int size = 0) : IEnumerator<T>
{
    private readonly T[]? pool = pool;
    private int index = 0;
    private readonly int size = size == 0 ? pool.Length : size;

    public T Current
    {
        get
        {
            if (pool == null || index == 0)
            {
                throw new InvalidOperationException();
            }

            return pool[index - 1];
        }
    }

    public bool MoveNext()
    {
        index++;
        return pool != null && size >= index;
    }

    public void Reset()
    {
        index = 0;
    }

    object? IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}
