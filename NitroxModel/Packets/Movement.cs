using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class Movement : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual NitroxVector3 Position { get; protected set; }
        [Index(2)]
        public virtual NitroxVector3 Velocity { get; protected set; }
        [Index(3)]
        public virtual NitroxQuaternion BodyRotation { get; protected set; }
        [Index(4)]
        public virtual NitroxQuaternion AimingRotation { get; protected set; }

        public Movement() { }

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
