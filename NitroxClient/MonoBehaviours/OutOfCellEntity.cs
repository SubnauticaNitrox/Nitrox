using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Entities might move slightly out of the loaded zone, in which case the server thinks that they're in another cell
/// (because the cell is only determined by the entity's position). Thus we need to be able to know when this entity is unloaded
/// and broadcast this event so the server can switch the ownership from it.
/// </summary>
public class OutOfCellEntity : MonoBehaviour
{
    private NitroxId entityId;

    public void Init(NitroxId nitroxId)
    {
        if (entityId == null)
        {
            this.Resolve<IPacketSender>().Send(new PlayerSeeOutOfCellEntity(nitroxId));
        }
        entityId = nitroxId;
    }

    public void OnDestroy()
    {
        this.Resolve<IPacketSender>().Send(new PlayerUnseeOutOfCellEntity(entityId));
    }
}
