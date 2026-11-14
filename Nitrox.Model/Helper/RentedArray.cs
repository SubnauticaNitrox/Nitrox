using System;
using System.Buffers;

namespace Nitrox.Model.Helper;

public readonly record struct RentedArray<T>(int RequiredSize) : IDisposable
{
    public readonly T[] Value = ArrayPool<T>.Shared.Rent(RequiredSize);

    public void Dispose() => ArrayPool<T>.Shared.Return(Value, true);

    public StructEnumerator<T> GetEnumerator() => new(Value, RequiredSize);
}
