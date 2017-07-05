using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Vector3 PlayerPosition { get; protected set; }
        public Quaternion Rotation { get; protected set; }

        public Movement(String playerId, Vector3 playerPosition, Quaternion rotation) : base(playerId, playerPosition)
        {
            this.PlayerPosition = playerPosition;
            this.Rotation = rotation;
            this.PlayerMustBeInRangeToReceive = false;
        }
    }
}
