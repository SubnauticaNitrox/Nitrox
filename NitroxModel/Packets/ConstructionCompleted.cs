using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : PlayerActionPacket
    {
        public string Guid { get; }
        public Optional<string> NewBaseCreatedGuid { get; }

        public ConstructionCompleted(Vector3 itemPosition, string guid, Optional<string> newBaseCreatedGuid) : base(itemPosition)
        {
            Guid = guid;
            NewBaseCreatedGuid = newBaseCreatedGuid;
        }

        public override string ToString()
        {
            return "[ConstructionCompleted Guid: " + Guid + " NewBaseCreatedGuid: " + NewBaseCreatedGuid + "]";
        }
    }
}
