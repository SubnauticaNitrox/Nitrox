using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class PrecursorDoorwayMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual bool IsOpen { get; protected set; }

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
