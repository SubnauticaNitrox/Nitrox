using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : PlayerActionPacket
    { 
        public String Guid { get; private set; }
        public Optional<String> NewBaseCreatedGuid { get; private set; }

        public ConstructionCompleted(String playerId, Vector3 itemPosition, String guid, Optional<String> newBaseCreatedGuid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.NewBaseCreatedGuid = newBaseCreatedGuid;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[ConstructionCompleted( - playerId: " + PlayerId + " Guid: " + Guid + " NewBaseCreatedGuid: " + NewBaseCreatedGuid + "]";
        }
    }
}
