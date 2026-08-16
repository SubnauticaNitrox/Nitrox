using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Entities might move slightly out of the loaded zone, in which case the server thinks that they're in another cell
/// (because the cell is only determined by the entity's position). Thus we need to be able to know when this entity is unloaded
/// and broadcast this event so the server can switch the ownership from it.
/// </summary>
internal sealed class OutOfCellEntity : MonoBehaviour
{
    private NitroxId? entityId;
    private IPacketSender packetSender;

    public void Init(NitroxId nitroxId, IPacketSender sender)
    {
        packetSender = sender;
        if (entityId == null)
        {
            packetSender.Send(new PlayerSeeOutOfCellEntity(nitroxId));
        }
        entityId = nitroxId;
    }

    public void OnDestroy()
    {
        packetSender.Send(new PlayerUnseeOutOfCellEntity(entityId!)); // entityId is not null after "Init" call.
    }
}
