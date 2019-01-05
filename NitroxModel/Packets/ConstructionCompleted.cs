using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : Packet
    {
        public string Guid { get; }
        public Optional<string> NewBaseCreatedGuid { get; }

        public ConstructionCompleted(string guid, Optional<string> newBaseCreatedGuid)
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
