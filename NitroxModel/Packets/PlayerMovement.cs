using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerMovement : Movement
    {
        public override ushort PlayerId { get; }
        public override NitroxVector3 Position { get; }
        public override NitroxQuaternion BodyRotation { get; }
        public override NitroxQuaternion AimingRotation { get; }

        public PlayerMovement(ushort playerId, NitroxVector3 position, NitroxQuaternion bodyRotation, NitroxQuaternion aimingRotation)
        {
            PlayerId = playerId;
            Position = position;
            BodyRotation = bodyRotation;
            AimingRotation = aimingRotation;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.PLAYER_MOVEMENT;
        }
    }
}
