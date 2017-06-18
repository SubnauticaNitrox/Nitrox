using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerActionPacket : Packet
    {
        public Vector3 ActionPosition { get; protected set; }
        public bool PlayerMustBeInRangeToReceive { get; protected set; }

        public PlayerActionPacket(String playerId, Vector3 eventPosition) : base(playerId)
        {
            this.ActionPosition = eventPosition;
            this.PlayerMustBeInRangeToReceive = true;
        }
    }
}
