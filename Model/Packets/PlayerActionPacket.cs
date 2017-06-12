using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerActionPacket : Packet
    {
        public Vector3 PlayerPosition { get; protected set; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }
        public bool IgnoreIfPlayerNotInRange { get; protected set; }

        public PlayerActionPacket(String playerId, Vector3 playerPosition) : base(playerId)
        {
            this.PlayerPosition = playerPosition;
            this.PlayerMustBeInRangeToReceive = true;
            this.IgnoreIfPlayerNotInRange = false;
        }
    }
}
