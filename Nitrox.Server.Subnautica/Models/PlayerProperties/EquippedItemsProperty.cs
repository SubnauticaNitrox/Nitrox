using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed class EquippedItemsProperty : IPlayerProperty<ThreadSafeDictionary<string, NitroxId>>
{
    public ThreadSafeDictionary<string, NitroxId> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
