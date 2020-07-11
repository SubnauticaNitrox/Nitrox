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

        protected PrecursorTeleporterMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
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
