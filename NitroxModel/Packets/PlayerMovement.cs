using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerMovement : Movement
    {
        public override NitroxId Id { get; }
        public override NitroxVector3 Position { get; }
        public override NitroxQuaternion Rotation { get; }
        public NitroxQuaternion AimingRotation { get; }

        public PlayerMovement(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxQuaternion aimingRotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            AimingRotation = aimingRotation;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.PLAYER_MOVEMENT;
        }
    }
}
