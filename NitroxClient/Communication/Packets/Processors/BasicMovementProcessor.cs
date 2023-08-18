using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class BasicMovementProcessor : ClientPacketProcessor<BasicMovement>
{
    public BasicMovementProcessor()
    {
    }

    public override void Process(BasicMovement movement)
    {
        if (NitroxEntity.TryGetObjectFrom(movement.Id, out GameObject gameObject))
        {
            MovementController mc = gameObject.EnsureComponent<MovementController>();
            mc.TargetPosition = movement.Position.ToUnity();
            mc.TargetRotation = movement.Rotation.ToUnity();
        }
    }
}
