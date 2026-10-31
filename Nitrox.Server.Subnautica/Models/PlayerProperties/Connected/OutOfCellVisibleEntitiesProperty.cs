using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record OutOfCellVisibleEntitiesProperty : IConnectedPlayerProperty<ThreadSafeSet<NitroxId>, OutOfCellVisibleEntitiesProperty>
{
    public ThreadSafeSet<NitroxId> Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    } = [];

    public Task ResetAsync()
    {
        Value.Clear();
        return Task.CompletedTask;
    }
}
