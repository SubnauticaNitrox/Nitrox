using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

/// <summary>
///     Is true if this player is using fresh data (first time visit).
/// </summary>
/// <remarks>
///     Not that same as <see cref="VisitCountProperty" /> because it might be older version of Nitrox that doesn't have
///     this yet. Or can just be unreliable if edited by an admin.
/// </remarks>
internal sealed record IsNewPlayerProperty : IConnectedPlayerProperty<bool, IsNewPlayerProperty>
{
    public bool Value
    {
        get => Interlocked.CompareExchange(ref field, true, true);
        set => Interlocked.Exchange(ref field, value);
    } = true;
}
