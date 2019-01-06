using System;
using UnityEngine;
using Lidgren.Network;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : Packet
    {
        public PlayerContext PlayerContext;
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }
        public Quaternion BodyRotation { get; }
        public Quaternion AimingRotation { get; }

        public Movement(PlayerContext playerContext, Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation)
        {
            PlayerContext = playerContext;
            Position = position;
            Velocity = velocity;
            BodyRotation = bodyRotation;
            AimingRotation = aimingRotation;
            DeliveryMethod = NetDeliveryMethod.UnreliableSequenced;
            UdpChannel = UdpChannelId.PLAYER_MOVEMENT;
        }

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerContext.PlayerId + " Position: " + Position + " Velocity: " + Velocity + " Body rotation: " + BodyRotation + " Camera rotation: " + AimingRotation + "]";
        }
    }
}
