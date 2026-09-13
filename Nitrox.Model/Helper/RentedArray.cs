using System;
using System.Buffers;

namespace Nitrox.Model.Helper;

public readonly struct RentedArray<T>(int rentAmount) : IDisposable
{
    public readonly T[] Value = ArrayPool<T>.Shared.Rent(rentAmount);

    public void Dispose() => ArrayPool<T>.Shared.Return(Value, true);

    public StructEnumerator<T> GetEnumerator() => new(Value, rentAmount);
}
