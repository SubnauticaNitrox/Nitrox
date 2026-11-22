using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed class UsedItemsProperty : IPlayerProperty<ThreadSafeSet<NitroxTechType>>
{
    public ThreadSafeSet<NitroxTechType> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
