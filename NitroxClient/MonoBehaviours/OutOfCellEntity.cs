using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
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
    private bool manuallyDestroyed;
    private NitroxId nitroxId;

    private SimulationOwnership SimulationOwnership => this.Resolve<SimulationOwnership>();

    public void Init(NitroxId nitroxId)
    {
        this.nitroxId = nitroxId;
    }

    /// <remarks>
    /// Once an entity out of cell is "free" from simulation ownership, we need to ask the server to lock it since we don't have
    /// it's actual cell loaded (from position)
    /// </remarks>
    public void TryClaim()
    {
        SimulationOwnership.RequestSimulationLock(nitroxId, SimulationLockType.TRANSIENT);
    }

    /// <summary>
    /// Remove the component without triggering the entity unload broadcast
    /// </summary>
    public void ManuallyDestroy()
    {
        manuallyDestroyed = true;
        Destroy(this);
    }

    public void OnDestroy()
    {
        if (manuallyDestroyed)
        {
            return;
        }

        // We check for IsAlive because we don't want to broadcast this when the entity was killed (it's managed in another way)
        if (TryGetComponent(out LiveMixin liveMixin) && liveMixin.IsAlive() && SimulationOwnership.HasAnyLockType(nitroxId))
        {
            SimulationOwnership.StopSimulatingEntity(nitroxId);
            EntityPositionBroadcaster.StopWatchingEntity(nitroxId);
            this.Resolve<IPacketSender>().Send(new DropSimulationOwnership(nitroxId));
        }
    }
}
