using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

/// <summary>
///     Ping reference instances.
/// </summary>
[Name("PingPreferences")]
[Group(PlayerService.PLAYER_PREFERENCES_GROUP_NAME)]
internal sealed class PingsProperty : IPlayerProperty<ThreadSafeDictionary<string, PingInstancePreference>>
{
    public ThreadSafeDictionary<string, PingInstancePreference> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
