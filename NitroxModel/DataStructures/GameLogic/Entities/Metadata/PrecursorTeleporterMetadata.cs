using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class PrecursorTeleporterMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public bool IsOpen { get; }

        public PrecursorTeleporterMetadata()
        {
            // Constructor for serialization
        }

        public PrecursorTeleporterMetadata(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public override string ToString()
        {
            return "[PrecursorTeleporterMetadata isOpen: " + IsOpen + "]";
        }
    }
}
