using NitroxClient.GameLogic;

namespace NitroxClient.MonoBehaviours.Vehicles;

public abstract class VehicleMovementReplicator : MovementReplicator
{
    public abstract void Enter(RemotePlayer remotePlayer);
    public abstract void Exit();
}
