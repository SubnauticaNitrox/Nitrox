using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BulkheadDoorRegisteredProcessor : IClientPacketProcessor<BulkheadDoorRegistered>
{
    public Task Process(ClientProcessorContext context, BulkheadDoorRegistered packet)
    {
        if (packet.Entity.ParentId == null || !NitroxEntity.TryGetComponentFrom(packet.Entity.ParentId, out Base @base))
        {
            // The base isn't spawned yet, its doors will come with the build entity
            return Task.CompletedTask;
        }

        @base.gameObject.EnsureComponent<BulkheadDoorManager>().RegisterDoor(packet.Entity);
        return Task.CompletedTask;
    }
}
