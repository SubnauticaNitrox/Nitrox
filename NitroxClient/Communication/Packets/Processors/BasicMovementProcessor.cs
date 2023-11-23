using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class BasicMovementProcessor : ClientPacketProcessor<BasicMovement>
{
    public override void Process(BasicMovement movement)
    {
        if (!NitroxEntity.TryGetMovementControllerFrom(movement.Id, out MovementController mc) && NitroxEntity.TryGetObjectFrom(movement.Id, out GameObject gameObject))
        {
            mc = gameObject.EnsureComponent<MovementController>();
        }

        if (mc)
        {
            mc.TargetPosition = movement.Position.ToUnity();
            mc.TargetRotation = movement.Rotation.ToUnity();
        }
    }
}
