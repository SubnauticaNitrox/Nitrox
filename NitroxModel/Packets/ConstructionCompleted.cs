using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : PlayerActionPacket
    { 
        public String Guid { get; }
        public Optional<String> NewBaseCreatedGuid { get; }

        public ConstructionCompleted(String playerId, Vector3 itemPosition, String guid, Optional<String> newBaseCreatedGuid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.NewBaseCreatedGuid = newBaseCreatedGuid;
        }

        public override string ToString()
        {
            return "[ConstructionCompleted( - playerId: " + PlayerId + " Guid: " + Guid + " NewBaseCreatedGuid: " + NewBaseCreatedGuid + "]";
        }
    }
}
