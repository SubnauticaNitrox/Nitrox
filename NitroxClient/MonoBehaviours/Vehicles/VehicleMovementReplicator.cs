using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public abstract class VehicleMovementReplicator : MovementReplicator
{
    protected static readonly int VIEW_YAW = Animator.StringToHash("view_yaw");
    protected static readonly int VIEW_PITCH = Animator.StringToHash("view_pitch");
    
    public abstract void Enter(RemotePlayer remotePlayer);
    public abstract void Exit();
}
