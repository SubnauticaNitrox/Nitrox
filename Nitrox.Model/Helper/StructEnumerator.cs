using System;
using System.Collections;
using System.Collections.Generic;

namespace Nitrox.Model.Helper;

/// <summary>
///     Allows non-alloc iteration.<br />
///     <b>Do not iterate this enumerator twice!</b>
///     The underlying disposable will be disposed after iteration completes.
/// </summary>
public struct StructEnumerator<T, TDisposable>(T[] pool, int actualSize = 0, TDisposable? innerDisposable = default) : IEnumerator<T>
    where TDisposable : IDisposable
{
    private readonly T[]? pool = pool;
    private int index = 0;
    private readonly int size = actualSize == 0 ? pool.Length : actualSize;
    private readonly WrappedDisposable innerDisposable = new(innerDisposable);

    public T Current
    {
        get
        {
            if (pool == null || index == 0)
            {
                ThrowHelper.ThrowInvalidOperation();
            }

            return pool[index - 1];
        }
    }

    public bool MoveNext()
    {
        if (innerDisposable.IsDisposed)
        {
            ThrowHelper.ThrowAlreadyDisposed(nameof(StructEnumerator<>));
        }
        if (pool is not [_, ..])
        {
            return false;
        }
        if (++index <= size)
        {
            return true;
        }
        return false;
    }

    public void Reset()
    {
        index = 0;
    }

    public readonly StructEnumerator<T, TDisposable> GetEnumerator() => this;
    object? IEnumerator.Current => throw new NotSupportedException();

    public void Dispose()
    {
        if (innerDisposable.IsDisposed)
        {
            ThrowHelper.ThrowAlreadyDisposed(nameof(StructEnumerator<>));
        }
        innerDisposable.Dispose();
    }

    public class WrappedDisposable(TDisposable? disposable) : IDisposable
    {
        private readonly TDisposable? disposable = disposable;
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            disposable?.Dispose();
        }
    }
}

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
                ThrowHelper.ThrowInvalidOperation();
            }

            return pool[index - 1];
        }
    }

    public bool MoveNext()
    {
        index++;
        return pool is not (null or []) && size >= index;
    }

    public void Reset()
    {
        index = 0;
    }

    public readonly StructEnumerator<T> GetEnumerator() => this;

    object? IEnumerator.Current => throw new NotSupportedException();

    public void Dispose()
    {
    }
}
