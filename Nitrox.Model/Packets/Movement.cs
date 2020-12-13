using System;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Networking;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerId + " Position: " + Position + " Velocity: " + Velocity + " Body rotation: " + BodyRotation + " Camera rotation: " + AimingRotation + "]";
        }
    }
}
