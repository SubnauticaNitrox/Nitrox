using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed class IsMutedProperty : IConnectedPlayerProperty<bool, IsMutedProperty>
{
    public bool Value
    {
        get => Interlocked.CompareExchange(ref field, true, true);
        set => Interlocked.Exchange(ref field, value);
    }
}
