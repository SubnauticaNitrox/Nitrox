using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Vector3 PlayerPosition { get; protected set; }
        public Quaternion BodyRotation { get; protected set; }
        public Quaternion CameraRotation { get; protected set; }
        public Optional<String> SubGuid { get; protected set; }

        public Movement(String playerId, Vector3 playerPosition, Quaternion bodyRotation, Quaternion cameraRotation, Optional<String> subGuid) : base(playerId, playerPosition)
        {
            this.PlayerPosition = playerPosition;
            this.BodyRotation = bodyRotation;
            this.CameraRotation = cameraRotation;
            this.SubGuid = subGuid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerId + " PlayerPosition: " + PlayerPosition + " Body rotation: " + BodyRotation + " Camera rotation: " + CameraRotation + " SubGuid: " + SubGuid + "]";
        }
    }
}
