using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed record DisplaySurfaceWaterProperty : IPlayerProperty<bool>
{
    public bool Value
    {
        get => Interlocked.CompareExchange(ref field, false, false);
        set => Interlocked.Exchange(ref field, value);
    } = true;
}
