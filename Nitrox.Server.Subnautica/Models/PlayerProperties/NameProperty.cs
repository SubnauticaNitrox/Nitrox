using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed class NameProperty : IPlayerProperty<string>
{
    public string Value
    {
        get => Interlocked.CompareExchange(ref field, "", "");
        set => Interlocked.Exchange(ref field, value);
    } = "";
}
