using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed record VisitCountProperty : IPlayerProperty<uint>
{
    private uint value;

    public uint Value
    {
        get => Interlocked.CompareExchange(ref value, 0, 0);
        set => Interlocked.Exchange(ref this.value, value);
    }

    public uint Increment() => Interlocked.Increment(ref value);
}
