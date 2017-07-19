using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Vector3 PlayerPosition { get; protected set; }
        public Quaternion Rotation { get; protected set; }
        public Optional<String> SubGuid { get; protected set; }

        public Movement(String playerId, Vector3 playerPosition, Quaternion rotation, Optional<String> subGuid) : base(playerId, playerPosition)
        {
            this.PlayerPosition = playerPosition;
            this.Rotation = rotation;
            this.SubGuid = subGuid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[Movement - PlayerId: " + PlayerId + " PlayerPosition: " + PlayerPosition + " Rotation: " + Rotation + " SubGuid: " + SubGuid + "]";
        }
    }
}
