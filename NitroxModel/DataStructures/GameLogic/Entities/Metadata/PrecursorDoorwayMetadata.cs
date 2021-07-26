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

        protected PrecursorDoorwayMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
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
