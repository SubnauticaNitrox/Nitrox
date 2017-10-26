using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }
        public Quaternion BodyRotation { get; }
        public Quaternion AimingRotation { get; }
        public Optional<String> SubGuid { get; }

        public Movement(String playerId, Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<String> subGuid) : base(playerId, position)
        {
            Position = position;
            Velocity = velocity;
            BodyRotation = bodyRotation;
            AimingRotation = aimingRotation;
            SubGuid = subGuid;
            PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerId + " Position: " + Position + " Velocity: " + Velocity + " Body rotation: " + BodyRotation + " Camera rotation: " + AimingRotation + " SubGuid: " + SubGuid + "]";
        }
    }
}
