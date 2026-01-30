using System.Buffers;

namespace Nitrox.Server.Subnautica.Models.Helper;

internal static class EasyPool<T>
{
    private static readonly ArrayPool<T> pool = ArrayPool<T>.Create(1, 1);

    public static Lease Rent() => new(pool.Rent(1));

    internal readonly struct Lease(T[] rentedArray) : IDisposable
    {
        private readonly T[] rentedArray = rentedArray;

        public ref T GetRef() => ref rentedArray[0];

        public void Dispose() => pool.Return(rentedArray);
    }
}
