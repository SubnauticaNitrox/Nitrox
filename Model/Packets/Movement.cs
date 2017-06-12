using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Movement : PlayerActionPacket
    {
        public Movement(String playerId, Vector3 playerPosition) : base(playerId, playerPosition)
        {
            this.IgnoreIfPlayerNotInRange = true;
        }
    }
}
