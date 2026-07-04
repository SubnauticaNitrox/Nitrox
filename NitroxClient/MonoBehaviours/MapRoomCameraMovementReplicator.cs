using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.MonoBehaviours;

public sealed class MapRoomCameraMovementReplicator : MovementReplicator
{
    public override void ApplyNewMovementData(MovementData newMovementData)
    {
        // MapRoomCamera does not need steering wheel / IK / animation side effects.
        // Base MovementReplicator handles buffered position + rotation interpolation.
    }
}
