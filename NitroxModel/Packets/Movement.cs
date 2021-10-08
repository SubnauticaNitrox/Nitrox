using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : Packet
    {
        public ushort PlayerId { get; }
        public NitroxVector3 Position { get; }
        public NitroxVector3 Velocity { get; }
        public NitroxQuaternion BodyRotation { get; }
        public NitroxQuaternion AimingRotation { get; }

        public Movement(ushort playerId, NitroxVector3 position, NitroxVector3 velocity, NitroxQuaternion bodyRotation, NitroxQuaternion aimingRotation)
        {
            PlayerId = playerId;
            Position = position;
            Velocity = velocity;
            BodyRotation = bodyRotation;
            AimingRotation = aimingRotation;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.PLAYER_MOVEMENT;
        }
    }
}
