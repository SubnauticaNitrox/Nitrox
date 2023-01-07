using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityReparentedProcessor : ClientPacketProcessor<EntityReparented>
{
    public override void Process(EntityReparented packet)
    {
        GameObject entity = NitroxEntity.RequireObjectFrom(packet.Id);
        GameObject newParent = NitroxEntity.RequireObjectFrom(packet.NewParentId);

        entity.transform.SetParent(newParent.transform, false);
    }
}
