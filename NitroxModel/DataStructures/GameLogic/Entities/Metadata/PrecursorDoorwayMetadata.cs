using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class PrecursorDoorwayMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool IsOpen { get; }

        public PrecursorDoorwayMetadata()
        {
            // Constructor for serialization
        }

        public PrecursorDoorwayMetadata(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public override string ToString()
        {
            return "[PrecursorDoorwayMetadata isOpen: " + IsOpen + "]";
        }
    }
}
