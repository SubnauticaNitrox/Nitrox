using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Vector3 Position { get { return serializablePosition.ToVector3(); } }
        public Vector3 Velocity { get { return serializableVelocity.ToVector3(); } }
        public Quaternion BodyRotation { get { return serializableBodyRotation.ToQuaternion(); } }
        public Quaternion AimingRotation { get { return serializableAimingRotation.ToQuaternion(); } }
        public Optional<String> SubGuid { get; }

        private SerializableVector3 serializablePosition;
        private SerializableVector3 serializableVelocity;
        private SerializableQuaternion serializableBodyRotation;
        private SerializableQuaternion serializableAimingRotation;

        public Movement(String playerId, Vector3 position, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<String> subGuid) : base(playerId, position)
        {
            this.serializablePosition = SerializableVector3.from(position);
            this.serializableVelocity = SerializableVector3.from(velocity);
            this.serializableBodyRotation = SerializableQuaternion.from(bodyRotation);
            this.serializableAimingRotation = SerializableQuaternion.from(aimingRotation);
            this.SubGuid = subGuid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerId + " Position: " + serializablePosition + " Velocity: " + serializableVelocity + " Body rotation: " + serializableBodyRotation + " Camera rotation: " + serializableAimingRotation + " SubGuid: " + SubGuid + "]";
        }
    }
}
