using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("PinnedTechTypes")]
[Group(PlayerService.PLAYER_PREFERENCES_GROUP_NAME)]
internal sealed class PinnedRecipesProperty : IPlayerProperty<ThreadSafeList<int>>
{
    public ThreadSafeList<int> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
