using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("QuickSlotsBindingIds")]
internal sealed class QuickSlotsProperty : IPlayerProperty<ThreadSafeList<NitroxId>>
{
    public ThreadSafeList<NitroxId?> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
